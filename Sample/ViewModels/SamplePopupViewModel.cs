using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sample.ContentPages;
using ShellInject;

namespace Sample.ViewModels;

public partial class SamplePopupViewModel : BaseViewModel
{
    [ObservableProperty] private string _message = "Loading popup parameter...";
    
    public override async Task DataReceivedAsync(object? parameter)
    {
        if (parameter is not string message)
        {
            return;
        }

        await Task.Delay(500);
        Message = message;
    }
    
    [RelayCommand]
    private Task OnDismissAsync()
    {
        return ShellNavigation.DismissPopupAsync<SamplePopup>(data: "DismissPopupAsync returned this data from the popup.");
    }
}
