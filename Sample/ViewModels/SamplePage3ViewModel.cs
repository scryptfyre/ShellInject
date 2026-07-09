using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sample.ContentPages;
using ShellInject;

namespace Sample.ViewModels;

public partial class SamplePage3ViewModel : BaseViewModel
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
    private Task OnCloseAsync()
    {
        return ShellNavigation.PopToRootAsync(parameter: "PopToRootAsync closed the multi-page stack.");
    }
    
    [RelayCommand]
    private Task OnPushAsync()
    {
        return ShellNavigation.PushAsync<SamplePage2>(parameter: "SamplePage3 pushed another SamplePage2.");
    }
}
