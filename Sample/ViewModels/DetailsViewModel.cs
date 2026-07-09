using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sample.ContentPages;
using ShellInject;

namespace Sample.ViewModels;

public partial class DetailsViewModel : BaseViewModel
{
    [ObservableProperty] private string _dataReceivedText = "Waiting for DataReceivedAsync...";
    [ObservableProperty] private string _lifecycleText = "Created by ShellInject ViewModelType binding.";

    public override void OnAppearing()
    {
        LifecycleText = "OnAppearing ran from ShellInjectViewModel.";
        base.OnAppearing();
    }

    public override void OnDisAppearing()
    {
        base.OnDisAppearing();
    }

    public override Task OnAppearedAsync()
    {
        LifecycleText = "OnAppearedAsync ran after this page appeared.";
        return base.OnAppearedAsync();
    }
    
    public override Task DataReceivedAsync(object? parameter)
    {
        if (parameter is string data)
        {
            DataReceivedText = data;
        }
        
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task OnPopWithParameterAsync()
    {
        return ShellNavigation.PopAsync(parameter: "PopAsync returned this data from DetailsPage.");
    }

    [RelayCommand]
    private Task OnSendDataToMainAsync()
    {
        return ShellNavigation.SendDataToPageAsync<MainPage>(data: "SendDataToPageAsync sent this directly to MainViewModel.");
    }

    [RelayCommand]
    private Task OnPopToRootAsync()
    {
        return ShellNavigation.PopToRootAsync(parameter: "PopToRootAsync returned to the root with this data.");
    }
}
