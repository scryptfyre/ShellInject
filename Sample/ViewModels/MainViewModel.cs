using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sample.ContentPages;
using Sample.Services;
using ShellInject;

namespace Sample.ViewModels;

public partial class MainViewModel(ISampleService sampleService) : BaseViewModel
{
    private readonly ISampleService _sampleService = sampleService;

    [ObservableProperty] private string _reverseDataText = "Nothing returned yet. Try an example below.";
    [ObservableProperty] private string _lifecycleText = "Waiting for initialization...";

    public override void OnAppearing()
    {
        base.OnAppearing();
    }

    public override void OnDisAppearing()
    {
        base.OnDisAppearing();
    }

    public override Task OnAppearedAsync()
    {
        LifecycleText = "OnAppearedAsync ran after the page appeared.";
        return base.OnAppearedAsync();
    }

    public override Task InitializedAsync()
    {
        Debug.WriteLine($"SampleService: {_sampleService.GetMessage()}");
        LifecycleText = $"InitializedAsync resolved ISampleService: {_sampleService.GetMessage()}";
        return Task.CompletedTask;
    }

    public override Task ReverseDataReceivedAsync(object? parameter)
    {
        if (parameter is string text)
        {
            ReverseDataText = text;
        }
        return Task.CompletedTask;
    }

    public override Task DataReceivedAsync(object? parameter)
    {
        if (parameter is string text)
        {
            ReverseDataText = text;
        }

        Debug.WriteLine($"DataReceivedAsync: {parameter}");
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task OnShowDetailsAsync()
    {
        ResetResult();
        return ShellNavigation.PushAsync<DetailsPage>(parameter: "PushAsync parameter from the examples page.");
    }
    
    [RelayCommand]
    private Task OnPushModalAsync()
    {
        ResetResult();
        return ShellNavigation.PushModalAsync<DetailsPage>(parameter: "PushModalAsync parameter from the examples page.");
    }
    
    [RelayCommand]
    private async Task OnShowPopupAsync()
    {
        ResetResult();
        await ShellNavigation.ShowPopupAsync<SamplePopup>(data: "This popup received data through DataReceivedAsync.");
    }
    
    [RelayCommand]
    private Task OnNavigateTestAsync()
    {
        ResetResult();
        return ShellNavigation.PushModalWithNavigationAsync(
            page: new SamplePage2(),
            parameter: "Modal NavigationPage root parameter.");
    }

    [RelayCommand]
    private async Task ReplaceContent()
    {
        ResetResult();
        await ShellNavigation.ReplaceAsync<FlyoutPageThree>(parameter: "ReplaceAsync swapped the Shell content and delivered this parameter.");
    }

    [RelayCommand]
    private Task OnChangeTabAsync()
    {
        ResetResult();
        return ShellNavigation.ChangeTabAsync(
            tabIndex: 1,
            parameter: "ChangeTabAsync selected tab two and delivered this parameter.");
    }

    [RelayCommand]
    private Task OnPushMultiStackAsync()
    {
        ResetResult();
        return ShellNavigation.PushMultiStackAsync(
            pageTypes: [typeof(DetailsPage), typeof(SamplePage2), typeof(SamplePage3)],
            parameter: "PushMultiStackAsync delivered this parameter to the stack.");
    }

    private void ResetResult()
    {
        ReverseDataText = "Waiting for data to come back...";
    }
}
