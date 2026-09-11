using CommunityToolkit.Mvvm.Input;
using Sample.ContentPages;
using Sample.Models;
using Sample.Services;
using ShellInject;

namespace Sample.ViewModels;

public partial class FlyoutPageThreeViewModel(DemoSession session) : BaseViewModel(session)
{
    [RelayCommand]
    private Task ReturnToLabAsync() => RunAsync("ReplaceAsync<MainPage>", () =>
        ShellNavigation.ReplaceAsync<MainPage>(parameter: new DemoResult(Session.ValidReference, "Shell destination replaced successfully")));
}
