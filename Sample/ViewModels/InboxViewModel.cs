using CommunityToolkit.Mvvm.Input;
using Sample.ContentPages;
using Sample.Services;
using ShellInject;

namespace Sample.ViewModels;

// Explicit XAML ViewModelType demonstrates binding a page to a differently named ViewModel.
public partial class InboxViewModel(DemoSession session) : BaseViewModel(session)
{
    [RelayCommand]
    private Task OpenOverviewAsync() => RunAsync("ChangeTabAsync<TabbedPageOne>", () =>
        ShellNavigation.ChangeTabAsync<TabbedPageOne>(parameter: $"{Session.ValidReference} · Sent from Inbox"));

    [RelayCommand]
    private Task ReturnToLabAsync() => RunAsync("ReplaceAsync<MainPage>", () =>
        ShellNavigation.ReplaceAsync<MainPage>(parameter: "Tab workflow completed"));
}
