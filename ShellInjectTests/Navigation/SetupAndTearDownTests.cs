using ShellInject.Navigation;

namespace ShellInjectTests.Navigation;

public class SetupAndTearDownTests
{
    [Fact]
    public void ShellTeardown_ValuesShouldBeDefault()
    {
        var testShell = new Shell();
        var nav = new ShellInjectNavigation
        {
            IsReverseNavigation = true,
            NavigationParameter = new object(),
            Shell = testShell,
            NavigatedHandler = void (_, _) => { }
        };

        nav.ShellTeardown(testShell);
        
        Assert.False(nav.IsReverseNavigation);
        Assert.Null(nav.NavigationParameter);
        Assert.Null(nav.Shell);
        Assert.Null(nav.NavigatedHandler);
    }
}