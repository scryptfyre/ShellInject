using CommunityToolkit.Mvvm.ComponentModel;
using Sample.Services;
using ShellInject;

namespace Sample.ViewModels;

public partial class BaseViewModel(DemoSession session) : ShellInjectViewModel
{
    public DemoSession Session { get; } = session;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(CanInteract))] private bool _isBusy;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _dataReceivedText = "No forward parameter yet. Start this workflow from the lab.";
    [ObservableProperty] private string _reverseDataText = "No result returned to this step yet.";
    public bool CanInteract => !IsBusy;

    // Keep each example's actual API call in its own ViewModel; only UI error/busy handling is shared.
    protected async Task RunAsync(string action, Func<Task> operation)
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = string.Empty;
        Session.Record(GetType().Name, action);
        try { await operation(); }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not complete this action: {ex.Message}";
            Session.Record(GetType().Name, "Error", ex.Message);
        }
        finally { IsBusy = false; }
    }

    public override void OnAppearing() => Session.Record(GetType().Name, "OnAppearing");
    public override void OnDisappearing() => Session.Record(GetType().Name, "OnDisappearing");
    public override Task OnAppearedAsync() { Session.Record(GetType().Name, "OnAppearedAsync"); return Task.CompletedTask; }
    public override Task InitializedAsync() { Session.Record(GetType().Name, "InitializedAsync", "Once per ViewModel instance"); return Task.CompletedTask; }
    public override Task DataReceivedAsync(object? parameter)
    {
        DataReceivedText = parameter?.ToString() ?? "No parameter";
        Session.Record(GetType().Name, "DataReceivedAsync", parameter);
        return Task.CompletedTask;
    }
    public override Task ReverseDataReceivedAsync(object? parameter)
    {
        ReverseDataText = parameter?.ToString() ?? "No parameter";
        Session.Record(GetType().Name, "ReverseDataReceivedAsync", parameter);
        return Task.CompletedTask;
    }
}
