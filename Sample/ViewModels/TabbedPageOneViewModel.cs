using CommunityToolkit.Mvvm.Input;
using ShellInject;

namespace Sample.ViewModels;

public partial class TabbedPageOneViewModel : BaseViewModel
{
    [RelayCommand]
    private async Task ShowTabTwo()
    {
        await ShellNavigation.ChangeTabAsync(tabIndex: 1, parameter: "Tab Swapped");
    }
}
