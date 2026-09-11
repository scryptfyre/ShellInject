using CommunityToolkit.Mvvm.Input;
using Sample.ContentPages;
using Sample.Models;
using Sample.Services;
using ShellInject;

namespace Sample.ViewModels;

public partial class MainViewModel(DemoSession session, ISampleService sampleService) : BaseViewModel(session)
{
    public string ServiceMessage { get; } = sampleService.GetMessage();

    public override Task ReverseDataReceivedAsync(object? parameter)
    {
        Session.Receive("ReverseDataReceivedAsync", parameter);
        return Task.CompletedTask;
    }

    public override Task DataReceivedAsync(object? parameter)
    {
        Session.Receive("DataReceivedAsync · Shell replacement", parameter);
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task OpenDetailsAsync() => RunAsync("PushAsync<DetailsPage, DemoRequest>", () =>
        ShellNavigation.PushAsync<DetailsPage, DemoRequest>(new DemoRequest(Session.ValidReference, "Opened from the navigation lab")));

    [RelayCommand]
    private Task OpenModalAsync() => RunAsync("PushModalAsync<DetailsPage, DemoRequest>", () =>
        ShellNavigation.PushModalAsync<DetailsPage, DemoRequest>(new DemoRequest(Session.ValidReference, "Opened as a standalone modal", IsModal: true)));

    [RelayCommand]
    private Task OpenPopupAsync() => RunAsync("ShowPopupAsync<SamplePopup>", async () =>
    {
        await ShellNavigation.ShowPopupAsync<SamplePopup>(data: Session.ValidReference,
            onError: ex => Session.Record("Popup", "Error", ex.Message));
        Session.Record("Popup", "Closed", "If dismissed outside the popup, no return payload is sent.");
    });

    [RelayCommand]
    private Task OpenModalStackAsync() => RunAsync("PushModalWithNavigationAsync", () =>
        ShellNavigation.PushModalWithNavigationAsync(page: new ModalStackPage(), parameter: Session.ValidReference));

    [RelayCommand]
    private Task OpenStackAsync() => RunAsync("PushMultiStackAsync", () =>
        ShellNavigation.PushMultiStackAsync(pageTypes: [typeof(SamplePage2), typeof(SamplePage3)], parameter: Session.ValidReference));

    [RelayCommand]
    private Task OpenTabsAsync() => RunAsync("ChangeTabAsync · index 1", () =>
        ShellNavigation.ChangeTabAsync(tabIndex: 1, parameter: Session.ValidReference));

    [RelayCommand]
    private Task ReplaceContentAsync() => RunAsync("ReplaceAsync<FlyoutPageThree>", () =>
        ShellNavigation.ReplaceAsync<FlyoutPageThree>(parameter: Session.ValidReference));

    [RelayCommand]
    private Task OpenGuideAsync() => RunAsync("Open setup guide", () => ShellNavigation.PushAsync<GuidePage>());

    [RelayCommand]
    private Task OpenActivityAsync() => RunAsync("Open activity", () => ShellNavigation.PushAsync<ActivityPage>());
}
