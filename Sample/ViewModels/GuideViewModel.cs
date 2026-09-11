using CommunityToolkit.Mvvm.Input;
using Sample.ContentPages;
using Sample.Services;
using ShellInject;

namespace Sample.ViewModels;

public partial class GuideViewModel(DemoSession session) : BaseViewModel(session)
{
    [RelayCommand]
    private Task OpenActivityAsync() => RunAsync("Open activity", () => ShellNavigation.PushAsync<ActivityPage>());
}
