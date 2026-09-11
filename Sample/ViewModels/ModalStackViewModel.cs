using CommunityToolkit.Mvvm.Input;
using Sample.ContentPages;
using Sample.Models;
using Sample.Services;
using ShellInject;

namespace Sample.ViewModels;

public partial class ModalStackViewModel(DemoSession session) : BaseViewModel(session)
{
    [RelayCommand]
    private Task ReviewAsync() => RunAsync("Modal INavigation.PushAsync", async () =>
    {
        // ShellNavigation.PushAsync targets the Shell stack, NOT this nested NavigationPage.
        var modal = Shell.Current.Navigation.ModalStack.LastOrDefault() as NavigationPage
            ?? throw new InvalidOperationException("Open this example with PushModalWithNavigationAsync.");
        await modal.Navigation.PushAsync(new ModalStepPage());
    });

    [RelayCommand]
    private Task CloseStackAsync() => RunAsync("PopModalStackAsync", () =>
        ShellNavigation.PopModalStackAsync(data: new DemoResult(DataReceivedText, "Modal navigation stack closed")));
}
