using CommunityToolkit.Mvvm.Input;
using Sample.Services;
using ShellInject;

namespace Sample.ViewModels;

public partial class ModalStepViewModel(DemoSession session) : BaseViewModel(session)
{
    [RelayCommand]
    private Task ApproveAsync() => RunAsync("PopAsync · inside modal", () =>
        ShellNavigation.PopAsync(parameter: "Review approved. This result reached the modal root, not the lab."));

    [RelayCommand]
    private Task CloseAllAsync() => RunAsync("PopModalStackAsync · from nested page", () =>
        ShellNavigation.PopModalStackAsync(data: "Closed the entire modal stack from the review step"));
}
