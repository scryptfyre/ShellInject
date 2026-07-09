using CommunityToolkit.Mvvm.ComponentModel;

namespace Sample.ViewModels;

public partial class FlyoutPageThreeViewModel : BaseViewModel
{
    [ObservableProperty] private string _dataReceivedText = "Open this from the examples page to see ReplaceAsync data.";

    public override Task DataReceivedAsync(object? parameter)
    {
        if (parameter is string data)
        {
            DataReceivedText = data;
        }

        return Task.CompletedTask;
    }
}
