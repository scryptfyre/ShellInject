using CommunityToolkit.Mvvm.Input;
using Sample.ContentPages;
using Sample.Services;
using ShellInject;

namespace Sample.ViewModels;

public partial class TabbedPageOneViewModel(DemoSession session) : BaseViewModel(session)
{
    [RelayCommand]
    private Task OpenInboxAsync() => RunAsync("ChangeTabAsync · index 1", () =>
        ShellNavigation.ChangeTabAsync(tabIndex: 1, parameter: $"{Session.ValidReference} · Sent from Overview"));

    [RelayCommand]
    private Task ReturnToLabAsync() => RunAsync("ReplaceAsync<MainPage>", () =>
        ShellNavigation.ReplaceAsync<MainPage>(parameter: "Tab workflow completed"));
}
