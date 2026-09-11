using CommunityToolkit.Mvvm.Input;
using Sample.ContentPages;
using Sample.Models;
using Sample.Services;
using ShellInject;

namespace Sample.ViewModels;

public partial class SamplePage3ViewModel(DemoSession session) : BaseViewModel(session)
{
    [RelayCommand]
    private Task BackOneAsync() => RunAsync("PopAsync", () => ShellNavigation.PopAsync(parameter: "Review returned to the intermediate step"));

    [RelayCommand]
    private Task FinishAsync() => RunAsync("PopToRootAsync", () =>
        ShellNavigation.PopToRootAsync(parameter: new DemoResult(DataReceivedText, "Multi-page stack completed")));

    [RelayCommand]
    private Task PopToLabAsync() => RunAsync("PopToAsync<MainPage>", () =>
        ShellNavigation.PopToAsync<MainPage>(parameter: new DemoResult(DataReceivedText, "Returned to MainPage by type")));
}
