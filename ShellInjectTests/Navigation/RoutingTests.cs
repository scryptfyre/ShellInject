using ShellInject.Navigation;

namespace ShellInjectTests.Navigation;

public class RoutingTests : BaseNavigationTests
{
    private class NotAPage;

    private class RouteCollisionA
    {
        public class CollidingPage : ContentPage;
    }

    private class RouteCollisionB
    {
        public class CollidingPage : ContentPage;
    }

    [Fact]
    public void BuildRoute_ShouldReturnExpectedRoute()
    {
        var nav = new ShellInjectNavigation(); 
        var pageType = typeof(TestPage);
        var result = nav.RegisterRoute(pageType);
        var expectedResult = $"si_{pageType.Name}";
        Assert.Equal(expectedResult, result); 
    }

    [Fact]
    public void RegisterRoute_WhenCalledTwiceForSameType_ShouldReturnSameRoute()
    {
        var nav = new ShellInjectNavigation();
        var pageType = typeof(RouteCollisionA.CollidingPage);

        var firstRoute = nav.RegisterRoute(pageType);
        var secondRoute = nav.RegisterRoute(pageType);

        Assert.Equal(firstRoute, secondRoute);
    }

    [Fact]
    public void RegisterRoute_WhenPageNamesCollide_ShouldReturnUniqueRoutes()
    {
        var nav = new ShellInjectNavigation();

        var firstRoute = nav.RegisterRoute(typeof(RouteCollisionA.CollidingPage));
        var secondRoute = nav.RegisterRoute(typeof(RouteCollisionB.CollidingPage));

        Assert.NotEqual(firstRoute, secondRoute);
        Assert.Contains(nameof(RouteCollisionB), secondRoute);
    }

    [Fact]
    public void RegisterRoute_WhenTypeIsNotPage_ShouldThrow()
    {
        var nav = new ShellInjectNavigation();

        Assert.Throws<ArgumentException>(() => nav.RegisterRoute(typeof(NotAPage)));
    }
}
