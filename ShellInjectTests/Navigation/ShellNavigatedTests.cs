using Moq;
using ShellInject.Navigation;

namespace ShellInjectTests.Navigation;

public class ShellNavigatedTests : BaseNavigationTests
{
    [Fact]
    public async Task OnShellNavigatedAsync_WhenShellNavigates_CallsOnAppearedOnce()
    {
        SetupShell_Page_And_BindingContext();

        MockShellViewModel.Setup(vm => vm.OnAppearedAsync());
        
        var navMock = new Mock<ShellInjectNavigation> { CallBase = true };
        navMock.SetupProperty(p => p.Shell, TestShell);

        await navMock.Object.OnShellNavigatedAsync(TestShell, NavigatedEventArgsPush);

        MockShellViewModel.Verify(vm => vm.OnAppearedAsync(), Times.Once);
    }
    
    [Fact]
    public async Task OnShellNavigatedAsync_WhenShellNavigates_CallsInitializedAsyncOnce()
    {
        SetupShell_Page_And_BindingContext();

        MockShellViewModel.Setup(vm => vm.InitializedAsync());
        
        var navMock = new Mock<ShellInjectNavigation> { CallBase = true };
        navMock.SetupProperty(p => p.Shell, TestShell);

        await navMock.Object.OnShellNavigatedAsync(TestShell, NavigatedEventArgsPush);

        MockShellViewModel.Verify(vm => vm.InitializedAsync(), Times.Once);
    }
    
    [Fact]
    public async Task OnShellNavigatedAsync_WhenShellNavigates_ShouldCallInitializedAsyncOnlyOnce()
    {
        SetupShell_Page_And_BindingContext();

        MockShellViewModel.Setup(vm => vm.InitializedAsync());
        MockShellViewModel.SetupProperty(vm => vm.IsInitialized, false);
        
        var navMock = new Mock<ShellInjectNavigation> { CallBase = true };
        navMock.SetupProperty(p => p.Shell, TestShell);

        await navMock.Object.OnShellNavigatedAsync(TestShell, NavigatedEventArgsPush);
        // Call again to test the IsInitialized is being set properly and not calling the method twice
        await navMock.Object.OnShellNavigatedAsync(TestShell, NavigatedEventArgsPush);

        MockShellViewModel.Verify(vm => vm.InitializedAsync(), Times.Once);
    }
    
    [Fact]
    public async Task OnShellNavigatedAsync_WhenShellNavigatesWithParameter_CallsDataReceivedAsyncOnce()
    {
        SetupShell_Page_And_BindingContext();

        MockShellViewModel.Setup(vm => vm.DataReceivedAsync(TestNavigationParameter));
        
        var navMock = new Mock<ShellInjectNavigation> { CallBase = true };
        navMock.SetupProperty(p => p.Shell, TestShell);
        navMock.SetupProperty(p => p.NavigationParameter, TestNavigationParameter);

        await navMock.Object.OnShellNavigatedAsync(TestShell, NavigatedEventArgsPush);

        MockShellViewModel.Verify(vm => vm.DataReceivedAsync(TestNavigationParameter), Times.Once);
    }
    
    [Fact]
    public async Task OnShellNavigatedAsync_WhenReverseNavigationWithParameter_CallsReverseDataReceivedAsyncOnce()
    {
        SetupShell_Page_And_BindingContext();

        MockShellViewModel.Setup(vm => vm.ReverseDataReceivedAsync(TestNavigationParameter));
        
        var navMock = new Mock<ShellInjectNavigation> { CallBase = true };
        navMock.SetupProperty(p => p.Shell, TestShell);
        navMock.SetupProperty(p => p.NavigationParameter, TestNavigationParameter);
        navMock.SetupProperty(p => p.IsReverseNavigation, true);

        await navMock.Object.OnShellNavigatedAsync(TestShell, NavigatedEventArgsPush);

        MockShellViewModel.Verify(vm => vm.ReverseDataReceivedAsync(TestNavigationParameter), Times.Once);
    }
    
    [Fact]
    public async Task OnShellNavigatedAsync_WhenNoParameterProvided_ShouldNotCallDataReceivedMethods()
    {
        SetupShell_Page_And_BindingContext();

        MockShellViewModel.Setup(vm => vm.ReverseDataReceivedAsync(null));
        MockShellViewModel.Setup(vm => vm.DataReceivedAsync(null));
        
        var navMock = new Mock<ShellInjectNavigation> { CallBase = true };
        navMock.SetupProperty(p => p.Shell, TestShell);

        await navMock.Object.OnShellNavigatedAsync(TestShell, NavigatedEventArgsPush);

        MockShellViewModel.Verify(vm => vm.ReverseDataReceivedAsync(null), Times.Never);
        MockShellViewModel.Verify(vm => vm.DataReceivedAsync(null), Times.Never);
    }

    [Fact]
    public async Task PopToAsync_WhenTargetIsCurrentPage_ShouldSendReverseData()
    {
        SetupShell_Page_And_BindingContext();

        MockShellViewModel.Setup(vm => vm.ReverseDataReceivedAsync(TestNavigationParameter));

        var nav = new ShellInjectNavigation();

        await nav.PopToAsync(TestShell, TestPageType, TestNavigationParameter);

        MockShellViewModel.Verify(vm => vm.ReverseDataReceivedAsync(TestNavigationParameter), Times.Once);
    }
}
