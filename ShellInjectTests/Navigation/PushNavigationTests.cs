using Moq;
using ShellInject.Navigation;

namespace ShellInjectTests.Navigation;

public class PushNavigationTests : BaseNavigationTests
{
    [Fact]
    public async Task PushAsync_WhenCalled_ShouldInvokeSetNavigationParameterOnce()
    {
        SetupShell_Page_And_BindingContext();
        
        var navMock = new Mock<ShellInjectNavigation> { CallBase = true };
        navMock.Setup(m => m.PushAsync(TestShell, TestPageType, TestNavigationParameter, false));
        navMock.Setup(n => n.SetNavigationParameter(It.IsAny<object?>()));
        
        Assert.Null(navMock.Object.NavigationParameter); // Verify it is null to start with

        await navMock.Object.PushAsync(TestShell, TestPageType, TestNavigationParameter, false);
        
        navMock.Verify(n => n.PushAsync(It.IsAny<Shell>(), TestPageType, TestNavigationParameter, false), Times.Once);
        navMock.Verify(n => n.SetNavigationParameter(It.IsAny<object?>()), Times.Once);
        Assert.Null(navMock.Object.NavigationParameter);
    }
    
    [Fact]
    public async Task PushAsync_ShouldNotThrow_WhenParameterIsNull()
    {
        SetupShell_Page_And_BindingContext();
        
        var navMock = new Mock<ShellInjectNavigation> { CallBase = true };
        navMock.Setup(m => m.PushAsync(TestShell, TestPageType, null, false));
        
        var exception = await Record.ExceptionAsync(async () =>
        {
            await navMock.Object.PushAsync(TestShell, TestPageType, TestNavigationParameter, false);
        });
        
        Assert.Null(exception);
    }
}