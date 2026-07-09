using CommunityToolkit.Mvvm.ComponentModel;

namespace Sample.ViewModels;

public partial class TabbedPagedTwoViewModel : BaseViewModel
{
    [ObservableProperty] private string _dataReceivedText = "Use the examples page or tab one to send data here.";

    public override Task DataReceivedAsync(object? parameter)
    {
        if (parameter is string data)
        {
            DataReceivedText = data;
        }
        
        return Task.CompletedTask;
    }
}   
