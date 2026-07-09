using Microsoft.Extensions.DependencyInjection;
using ShellInject;
using ShellInject.Constants;
using ShellInject.Interfaces;
using ShellInject.Services;

namespace ShellInjectTests;

public class CoreApiTests : IDisposable
{
    private readonly IServiceProvider? _originalServiceProvider = ShellInjectInitializer.ServiceProvider;

    public void Dispose()
    {
        ShellInjectInitializer.ServiceProvider = _originalServiceProvider;
    }

    [Fact]
    public void Constants_ShouldExposeExpectedMessages()
    {
        Assert.Equal("An error occurred trying to navigate with Shell navigation", ShellInjectConstants.ShellNotFoundText);
        Assert.Equal("ContentPage was found Null.", ShellInjectConstants.NullContentPageExceptionText);
        Assert.Equal("Expected List of Navigation States was null. Please pass in the List of ShellNavigationState's (strings) you want to use.", ShellInjectConstants.NavigationStatesExceptionText);
    }

    [Fact]
    public void Injector_WhenServiceProviderIsInitialized_ShouldResolveServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<TestService>();
        ShellInjectInitializer.ServiceProvider = services.BuildServiceProvider();

        Assert.NotNull(Injector.GetRequiredService<TestService>());
        Assert.NotNull(Injector.GetService<TestService>());
        Assert.Null(Injector.GetService<MissingService>());
    }

    [Fact]
    public void Injector_WhenServiceProviderIsMissing_ShouldThrow()
    {
        ShellInjectInitializer.ServiceProvider = null;

        Assert.Throws<InvalidOperationException>(() => Injector.GetRequiredService<TestService>());
        Assert.Throws<InvalidOperationException>(() => Injector.GetService<TestService>());
    }

    [Fact]
    public void Injector_WhenRequiredServiceIsMissing_ShouldThrow()
    {
        ShellInjectInitializer.ServiceProvider = new ServiceCollection().BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() => Injector.GetRequiredService<TestService>());
    }

    [Fact]
    public void ShellInjectInitializer_ShouldStoreServiceProvider()
    {
        var provider = new ServiceCollection().BuildServiceProvider();
        var initializer = new ShellInjectInitializer();

        initializer.Initialize(provider);

        Assert.Same(provider, ShellInjectInitializer.ServiceProvider);
    }

    [Fact]
    public async Task ShellInjectViewModel_DefaultHooks_ShouldBeNoOpsAndCommandsShouldInvokeHooks()
    {
        var viewModel = new CountingViewModel();

        Assert.False(viewModel.IsInitialized);
        var shellInjectViewModel = (IShellInjectShellViewModel)viewModel;
        shellInjectViewModel.IsInitialized = true;
        Assert.True(shellInjectViewModel.IsInitialized);
        Assert.True(viewModel.IsInitialized);
        await viewModel.InitializedAsync();
        await viewModel.DataReceivedAsync(null);
        await viewModel.ReverseDataReceivedAsync(null);
        await viewModel.OnAppearedAsync();

        viewModel.OnAppearingCommand.Execute(null);
        viewModel.OnDisAppearingCommand.Execute(null);

        Assert.Equal(1, viewModel.AppearingCount);
        Assert.Equal(1, viewModel.DisappearingCount);
    }

    [Fact]
    public void ShellInjectViewModel_BaseLifecycleHooks_ShouldBeNoOps()
    {
        var viewModel = new ShellInjectViewModel();

        viewModel.OnAppearing();
        viewModel.OnDisAppearing();

        Assert.NotNull(viewModel.OnAppearingCommand);
        Assert.NotNull(viewModel.OnDisAppearingCommand);
    }

    private sealed class TestService;

    private sealed class MissingService;

    private sealed class CountingViewModel : ShellInjectViewModel
    {
        public int AppearingCount { get; private set; }
        public int DisappearingCount { get; private set; }

        public override void OnAppearing()
        {
            AppearingCount++;
        }

        public override void OnDisAppearing()
        {
            DisappearingCount++;
        }
    }
}
