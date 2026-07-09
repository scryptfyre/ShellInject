using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Sample.ContentPages;
using ShellInject;

namespace Sample.ViewModels;

public partial class SamplePage2ViewModel : BaseViewModel
{
    [ObservableProperty] private string _dataReceivedText = "Waiting for DataReceivedAsync...";

    public override Task DataReceivedAsync(object? parameter)
    {
        if (parameter is string data)
        {
            DataReceivedText = data;
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task OnPushAsync()
    {
        return ShellNavigation.PushAsync<SamplePage3>(parameter: "SamplePage2 pushed SamplePage3 with this parameter.");
    }
}
