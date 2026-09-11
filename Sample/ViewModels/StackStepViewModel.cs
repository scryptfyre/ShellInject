using CommunityToolkit.Mvvm.Input;
using Sample.ContentPages;
using Sample.Services;
using ShellInject;

namespace Sample.ViewModels;

// This intentionally non-conventional name is mapped with RegisterViewModel in MauiProgram.
public partial class StackStepViewModel(DemoSession session) : BaseViewModel(session)
{
    [RelayCommand]
    private Task ContinueAsync() => RunAsync("PushAsync<SamplePage3>", () =>
        ShellNavigation.PushAsync<SamplePage3>(parameter: Session.ValidReference));

    [RelayCommand]
    private Task ReturnToLabAsync() => RunAsync("PopToAsync<MainPage>", () =>
        ShellNavigation.PopToAsync<MainPage>(parameter: "Stack walkthrough complete"));
}
