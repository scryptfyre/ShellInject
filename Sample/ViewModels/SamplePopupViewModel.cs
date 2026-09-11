using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sample.ContentPages;
using Sample.Models;
using Sample.Services;
using ShellInject;

namespace Sample.ViewModels;

public partial class SamplePopupViewModel(DemoSession session) : BaseViewModel(session)
{
    [ObservableProperty] private string _reply = "Dispatch confirmed";

    [RelayCommand]
    private Task ConfirmAsync() => RunAsync("DismissPopupAsync<SamplePopup>", () =>
        ShellNavigation.DismissPopupAsync<SamplePopup>(data: new DemoResult(DataReceivedText,
            string.IsNullOrWhiteSpace(Reply) ? "Dispatch confirmed" : Reply.Trim())));

    [RelayCommand]
    private Task CancelAsync() => RunAsync("DismissPopupAsync · no data", () =>
        ShellNavigation.DismissPopupAsync<SamplePopup>());
}
