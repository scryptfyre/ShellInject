using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sample.ContentPages;
using Sample.Models;
using Sample.Services;
using ShellInject;

namespace Sample.ViewModels;

// Typed forward data, object result sent back to the lab's non-generic ViewModel.
public partial class DetailsViewModel(DemoSession session, ISampleService service) : ShellInjectViewModel<DemoRequest>
{
    public DemoSession Session { get; } = session;
    public string ServiceMessage { get; } = service.GetMessage();
    [ObservableProperty] private string _reference = "Waiting for request";
    [ObservableProperty] private string _note = "Data is delivered through DataReceivedAsync(DemoRequest).";
    [ObservableProperty] private string _resultText = "Shipment approved";
    [ObservableProperty] private string _status = "Edit the result above, then return it to the lab.";
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(CanInteract))] private bool _isBusy;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(CanSendDirectly)), NotifyPropertyChangedFor(nameof(Presentation))] private bool _isModal;
    public bool CanInteract => !IsBusy;
    public bool CanSendDirectly => !IsModal;
    public string Presentation => IsModal ? "MODAL · TYPED REQUEST" : "PUSH · TYPED REQUEST";

    public override Task DataReceivedAsync(DemoRequest? parameter)
    {
        if (parameter is null) return Task.CompletedTask;
        Reference = parameter.Reference;
        Note = parameter.Note;
        IsModal = parameter.IsModal;
        Session.Record(nameof(DetailsViewModel), "DataReceivedAsync(DemoRequest)", parameter);
        return Task.CompletedTask;
    }

    public override Task InitializedAsync() { Session.Record(nameof(DetailsViewModel), "InitializedAsync"); return Task.CompletedTask; }
    public override Task OnAppearedAsync() { Session.Record(nameof(DetailsViewModel), "OnAppearedAsync"); return Task.CompletedTask; }
    public override void OnAppearing() => Session.Record(nameof(DetailsViewModel), "OnAppearing");
    public override void OnDisappearing() => Session.Record(nameof(DetailsViewModel), "OnDisappearing");

    private DemoResult Result => new(Reference, string.IsNullOrWhiteSpace(ResultText) ? "Shipment approved" : ResultText.Trim());

    [RelayCommand]
    private Task ReturnAsync() => RunAsync(() => ShellNavigation.PopAsync(parameter: Result));

    [RelayCommand]
    private Task SendUpdateAsync() => RunAsync(async () =>
    {
        // This API searches the regular Shell stack, so this action is offered only in the push workflow.
        await ShellNavigation.SendDataToPageAsync<MainPage>(data: Result);
        Status = "Update delivered to MainViewModel. You are still on the details page.";
        Session.Record(nameof(DetailsViewModel), "SendDataToPageAsync<MainPage>", Result);
    });

    private async Task RunAsync(Func<Task> operation)
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = string.Empty;
        try { await operation(); }
        catch (Exception ex) { ErrorMessage = ex.Message; Session.Record(nameof(DetailsViewModel), "Error", ex.Message); }
        finally { IsBusy = false; }
    }
}
