using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ShellInject.Navigation;
using ShellInject.Services;

namespace ShellInjectTests.Navigation;

public class NavigationInternalsCoverageTests : IDisposable
{
    private readonly IServiceProvider? _originalServiceProvider = ShellInjectInitializer.ServiceProvider;

    public void Dispose()
    {
        ShellInjectInitializer.ServiceProvider = _originalServiceProvider;
    }

    [Fact]
    public void ShellSetup_WhenShellIsNull_ShouldThrow()
    {
        var nav = new ShellInjectNavigation();
        var method = typeof(ShellInjectNavigation).GetMethod("ShellSetup", BindingFlags.Instance | BindingFlags.NonPublic)!;

        var exception = Assert.Throws<TargetInvocationException>(() => method.Invoke(nav, [null, true]));

        Assert.IsType<ArgumentNullException>(exception.InnerException);
    }

    [Fact]
    public void RegisterRoute_WhenSimpleAndNamespacedRoutesCollide_ShouldUseGeneratedRoute()
    {
        var nav = new ShellInjectNavigation();
        var pageType = typeof(FallbackPage);
        Routing.RegisterRoute("si_FallbackPage", typeof(ExistingFallbackPage));
        Routing.RegisterRoute("si_ShellInjectTests_Navigation_NavigationInternalsCoverageTests_FallbackPage", typeof(ExistingFallbackPage));

        var route = nav.RegisterRoute(pageType);

        Assert.StartsWith("si_ShellInjectTests_Navigation_NavigationInternalsCoverageTests_FallbackPage_", route);
    }

    [Fact]
    public void CreateInstance_WhenServiceProviderExists_ShouldUseActivatorUtilities()
    {
        var services = new ServiceCollection();
        services.AddSingleton<TestDependency>();
        ShellInjectInitializer.ServiceProvider = services.BuildServiceProvider();
        var method = GetCreateInstanceMethod();

        var result = method.Invoke(null, [typeof(InjectedContentPage)]);

        var page = Assert.IsType<InjectedContentPage>(result);
        Assert.NotNull(page.Dependency);
    }

    [Fact]
    public void CreateInstance_WhenServiceProviderIsMissing_ShouldUseDefaultConstructor()
    {
        ShellInjectInitializer.ServiceProvider = null;
        var method = GetCreateInstanceMethod();

        var result = method.Invoke(null, [typeof(DefaultContentPage)]);

        Assert.IsType<DefaultContentPage>(result);
    }

    [Fact]
    public void CreateTypedInstance_WhenCreatedTypeDoesNotMatchExpectedType_ShouldThrow()
    {
        ShellInjectInitializer.ServiceProvider = null;
        var method = typeof(ShellInjectNavigation)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(m => m.Name == "CreateInstance" && m.IsGenericMethod)
            .MakeGenericMethod(typeof(ContentPage));

        var exception = Assert.Throws<TargetInvocationException>(() => method.Invoke(null, [typeof(PlainObject)]));

        Assert.IsType<ArgumentException>(exception.InnerException);
    }

    [Fact]
    public void CreateTypedInstance_WhenCreatedTypeMatchesExpectedType_ShouldReturnInstance()
    {
        ShellInjectInitializer.ServiceProvider = null;
        var method = typeof(ShellInjectNavigation)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(m => m.Name == "CreateInstance" && m.IsGenericMethod)
            .MakeGenericMethod(typeof(ContentPage));

        var result = method.Invoke(null, [typeof(DefaultContentPage)]);

        Assert.IsType<DefaultContentPage>(result);
    }

    [Fact]
    public void ResolveShellContentPage_WhenContentAndTemplateAreMissing_ShouldReturnNull()
    {
        var method = typeof(ShellInjectNavigation).GetMethod("ResolveShellContentPage", BindingFlags.Static | BindingFlags.NonPublic)!;

        var result = method.Invoke(null, [new ShellContent()]);

        Assert.Null(result);
    }

    private sealed class FallbackPage : ContentPage;

    private static MethodInfo GetCreateInstanceMethod()
    {
        return typeof(ShellInjectNavigation)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(m => m.Name == "CreateInstance" && !m.IsGenericMethod);
    }

    private sealed class ExistingFallbackPage : ContentPage;

    private sealed class TestDependency;

    private sealed class InjectedContentPage(TestDependency dependency) : ContentPage
    {
        public TestDependency Dependency { get; } = dependency;
    }

    private sealed class DefaultContentPage : ContentPage;

    private sealed class PlainObject;
}
