using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Maui.Views;
using ShellInject;
using ShellInject.Extensions;
using ShellInject.Services;

namespace ShellInjectTests;

public class ViewModelBindingTests : IDisposable
{
    private readonly IServiceProvider? _originalServiceProvider = ShellInjectInitializer.ServiceProvider;

    public void Dispose()
    {
        ShellInjectInitializer.ServiceProvider = _originalServiceProvider;
        ShellInjectInitializer.Options.AutoBindViewModelsByConvention = true;
        ShellInjectInitializer.Options.PageSuffix = "Page";
        ShellInjectInitializer.Options.ViewModelSuffix = "ViewModel";
    }

    [Fact]
    public void UseShellInject_WhenReflected_ShouldExposeOriginalOneParameterOverload()
    {
        var overload = typeof(ShellInjectMauiBuilderExtensions)
            .GetMethods()
            .SingleOrDefault(method =>
            {
                if (method.Name != nameof(ShellInjectMauiBuilderExtensions.UseShellInject))
                {
                    return false;
                }

                var parameters = method.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == typeof(MauiAppBuilder);
            });

        Assert.NotNull(overload);
    }

    [Fact]
    public void SetViewModelType_WhenServiceProviderIsAvailable_ShouldResolveViewModelFromDi()
    {
        var services = new ServiceCollection();
        services.AddSingleton<TestDependency>();
        services.AddTransient<InjectedViewModel>();
        ShellInjectInitializer.ServiceProvider = services.BuildServiceProvider();
        var page = new ContentPage();

        ShellInjectPageExtensions.SetViewModelType(page, typeof(InjectedViewModel));

        var viewModel = Assert.IsType<InjectedViewModel>(page.BindingContext);
        Assert.NotNull(viewModel.Dependency);
    }

    [Fact]
    public void SetViewModelType_WhenServiceProviderIsMissing_ShouldCreateViewModelWithDefaultConstructor()
    {
        ShellInjectInitializer.ServiceProvider = null;
        var page = new ContentPage();

        ShellInjectPageExtensions.SetViewModelType(page, typeof(DefaultViewModel));

        Assert.IsType<DefaultViewModel>(page.BindingContext);
        Assert.Equal(typeof(DefaultViewModel), ShellInjectPageExtensions.GetViewModelType(page));
    }

    [Fact]
    public void SetViewModelType_WhenTargetIsNotPage_ShouldSetBindingContext()
    {
        ShellInjectInitializer.ServiceProvider = null;
        BindableObject[] targets =
        [
            new Popup(),
            new ContentView(),
            new Shell(),
            new FlyoutPage(),
            new NavigationPage(new ContentPage()),
            new CollectionView(),
            new Grid()
        ];

        foreach (var target in targets)
        {
            ShellInjectPageExtensions.SetViewModelType(target, typeof(DefaultViewModel));

            var bindingContext = target switch
            {
                Popup popup => popup.BindingContext,
                VisualElement visualElement => visualElement.BindingContext,
                _ => null
            };
            Assert.IsType<DefaultViewModel>(bindingContext);
        }
    }

    [Fact]
    public void SetViewModelType_WhenNewValueIsNotType_ShouldLeaveBindingContextUnchanged()
    {
        ShellInjectInitializer.ServiceProvider = null;
        var page = new ContentPage();
        ShellInjectPageExtensions.SetViewModelType(page, typeof(DefaultViewModel));
        var originalBindingContext = page.BindingContext;

        page.SetValue(ShellInjectPageExtensions.ViewModelTypeProperty, null);

        Assert.Same(originalBindingContext, page.BindingContext);
    }

    [Fact]
    public void SetViewModelType_WhenViewModelCannotBeCreated_ShouldNotThrow()
    {
        ShellInjectInitializer.ServiceProvider = null;
        var page = new ContentPage();

        var exception = Record.Exception(() => ShellInjectPageExtensions.SetViewModelType(page, typeof(NoDefaultConstructorViewModel)));

        Assert.Null(exception);
        Assert.Null(page.BindingContext);
    }

    [Fact]
    public void SetViewModelType_WhenContentPageUsesShellInjectViewModel_ShouldAttachAndDetachLifecycleHandlers()
    {
        ShellInjectInitializer.ServiceProvider = null;
        var page = new ContentPage();

        ShellInjectPageExtensions.SetViewModelType(page, typeof(LifecycleViewModel));
        var firstViewModel = Assert.IsType<LifecycleViewModel>(page.BindingContext);

        ShellInjectPageExtensions.SetViewModelType(page, typeof(SecondLifecycleViewModel));
        var secondViewModel = Assert.IsType<SecondLifecycleViewModel>(page.BindingContext);

        Assert.NotSame(firstViewModel, secondViewModel);
    }

    [Fact]
    public void SetViewModelType_WhenReplacementCannotBeCreated_ShouldKeepExistingLifecycleHandlers()
    {
        ShellInjectInitializer.ServiceProvider = null;
        var page = new ContentPage();
        ShellInjectPageExtensions.SetViewModelType(page, typeof(LifecycleViewModel));
        var originalViewModel = Assert.IsType<LifecycleViewModel>(page.BindingContext);
        var originalLifecycleToken = GetPageLifecycleToken(page);

        ShellInjectPageExtensions.SetViewModelType(page, typeof(NoDefaultConstructorViewModel));

        Assert.Same(originalViewModel, page.BindingContext);
        Assert.NotNull(originalLifecycleToken);
        Assert.Same(originalLifecycleToken, GetPageLifecycleToken(page));
    }

    [Fact]
    public void TryBindViewModelByConvention_WhenViewModelMatchesPageName_ShouldSetBindingContext()
    {
        ShellInjectInitializer.ServiceProvider = null;
        var page = new ConventionPage();

        var bound = ShellInjectPageExtensions.TryBindViewModelByConvention(page);

        Assert.True(bound);
        Assert.IsType<ConventionViewModel>(page.BindingContext);
    }

    [Fact]
    public void TryBindViewModelByConvention_WhenViewModelMatchesFullPageName_ShouldSetBindingContext()
    {
        ShellInjectInitializer.ServiceProvider = null;
        var page = new FullNamePage();

        var bound = ShellInjectPageExtensions.TryBindViewModelByConvention(page);

        Assert.True(bound);
        Assert.IsType<FullNamePageViewModel>(page.BindingContext);
    }

    [Fact]
    public void TryBindViewModelByConvention_WhenBothCandidateNamesExist_ShouldPreferTrimmedPageName()
    {
        ShellInjectInitializer.ServiceProvider = null;
        var page = new BothCandidatesPage();

        var bound = ShellInjectPageExtensions.TryBindViewModelByConvention(page);

        Assert.True(bound);
        Assert.IsType<BothCandidatesViewModel>(page.BindingContext);
    }

    [Fact]
    public void TryBindViewModelByConvention_WhenBindingContextExists_ShouldNotReplaceBindingContext()
    {
        ShellInjectInitializer.ServiceProvider = null;
        var existingViewModel = new DefaultViewModel();
        var page = new ConventionPage { BindingContext = existingViewModel };

        var bound = ShellInjectPageExtensions.TryBindViewModelByConvention(page);

        Assert.False(bound);
        Assert.Same(existingViewModel, page.BindingContext);
    }

    [Fact]
    public void TryBindViewModelByConvention_WhenConventionBindingDisabled_ShouldNotBind()
    {
        ShellInjectInitializer.ServiceProvider = null;
        ShellInjectInitializer.Options.AutoBindViewModelsByConvention = false;
        var page = new ConventionPage();

        var bound = ShellInjectPageExtensions.TryBindViewModelByConvention(page);

        Assert.False(bound);
        Assert.Null(page.BindingContext);
    }

    [Fact]
    public void TryBindViewModelByConvention_WhenExplicitViewModelTypeIsSet_ShouldNotReplaceBindingContext()
    {
        ShellInjectInitializer.ServiceProvider = null;
        var page = new ConventionPage();
        ShellInjectPageExtensions.SetViewModelType(page, typeof(DefaultViewModel));
        var explicitViewModel = page.BindingContext;

        var bound = ShellInjectPageExtensions.TryBindViewModelByConvention(page);

        Assert.False(bound);
        Assert.Same(explicitViewModel, page.BindingContext);
    }

    [Fact]
    public void TryBindViewModelByConvention_WhenServiceProviderIsAvailable_ShouldResolveFromDi()
    {
        var services = new ServiceCollection();
        services.AddSingleton<TestDependency>();
        services.AddTransient<ConventionViewModel>();
        ShellInjectInitializer.ServiceProvider = services.BuildServiceProvider();
        var page = new ConventionPage();

        var bound = ShellInjectPageExtensions.TryBindViewModelByConvention(page);

        var viewModel = Assert.IsType<ConventionViewModel>(page.BindingContext);
        Assert.True(bound);
        Assert.NotNull(viewModel.Dependency);
    }

    [Fact]
    public void TryBindViewModelByConvention_WhenNoMatchExists_ShouldNotBind()
    {
        ShellInjectInitializer.ServiceProvider = null;
        var page = new MissingMatchPage();

        var bound = ShellInjectPageExtensions.TryBindViewModelByConvention(page);

        Assert.False(bound);
        Assert.Null(page.BindingContext);
    }

    [Fact]
    public void TryBindViewModelByConvention_WhenMultipleMatchesExist_ShouldNotBind()
    {
        ShellInjectInitializer.ServiceProvider = null;
        var page = new AmbiguousPage();

        var bound = ShellInjectPageExtensions.TryBindViewModelByConvention(page);

        Assert.False(bound);
        Assert.Null(page.BindingContext);
    }

    [Fact]
    public void TryBindViewModelByConvention_WhenCustomSuffixesAreConfigured_ShouldUseConfiguredNames()
    {
        ShellInjectInitializer.ServiceProvider = null;
        ShellInjectInitializer.Options.PageSuffix = "Screen";
        ShellInjectInitializer.Options.ViewModelSuffix = "Vm";
        var page = new CustomScreen();

        var bound = ShellInjectPageExtensions.TryBindViewModelByConvention(page);

        Assert.True(bound);
        Assert.IsType<CustomVm>(page.BindingContext);
    }

    [Fact]
    public void TryBindViewModelByConvention_WhenTargetIsPopup_ShouldSetPopupBindingContext()
    {
        ShellInjectInitializer.ServiceProvider = null;
        var popup = new ConventionPopup();

        var bound = ShellInjectPageExtensions.TryBindViewModelByConvention(popup);

        Assert.True(bound);
        Assert.IsType<ConventionPopupViewModel>(popup.BindingContext);
    }

    [Fact]
    public void TryBindViewModelByConvention_WhenConventionViewModelCannotBeCreated_ShouldReturnFalse()
    {
        ShellInjectInitializer.ServiceProvider = null;
        var page = new ConstructorFailurePage();

        var bound = ShellInjectPageExtensions.TryBindViewModelByConvention(page);

        Assert.False(bound);
        Assert.Null(page.BindingContext);
    }

    private sealed class TestDependency;

    private static object? GetPageLifecycleToken(ContentPage page)
    {
        var propertyField = typeof(ShellInjectPageExtensions)
            .GetField("PageLifecycleTokenProperty", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var property = Assert.IsType<BindableProperty>(propertyField?.GetValue(null));

        return page.GetValue(property);
    }

    private sealed class InjectedViewModel(TestDependency dependency)
    {
        public TestDependency Dependency { get; } = dependency;
    }

    private sealed class DefaultViewModel;

    private sealed class NoDefaultConstructorViewModel(string value)
    {
        public string Value { get; } = value;
    }

    private sealed class LifecycleViewModel : ShellInjectViewModel
    {
        public int AppearingCount { get; private set; }

        public override void OnAppearing()
        {
            AppearingCount++;
        }
    }

    private sealed class SecondLifecycleViewModel : ShellInjectViewModel
    {
    }

    private sealed class ConventionPage : ContentPage;

    private sealed class ConventionViewModel : ShellInjectViewModel
    {
        public ConventionViewModel()
        {
        }

        public ConventionViewModel(TestDependency dependency)
        {
            Dependency = dependency;
        }

        public TestDependency? Dependency { get; }
    }

    private sealed class FullNamePage : ContentPage;

    private sealed class FullNamePageViewModel : ShellInjectViewModel;

    private sealed class BothCandidatesPage : ContentPage;

    private sealed class BothCandidatesViewModel : ShellInjectViewModel;

    private sealed class BothCandidatesPageViewModel : ShellInjectViewModel;

    private sealed class MissingMatchPage : ContentPage;

    private sealed class AmbiguousPage : ContentPage;

    private sealed class FirstAmbiguousContainer
    {
        public sealed class AmbiguousViewModel : ShellInjectViewModel;
    }

    private sealed class SecondAmbiguousContainer
    {
        public sealed class AmbiguousViewModel : ShellInjectViewModel;
    }

    private sealed class CustomScreen : ContentPage;

    private sealed class CustomVm : ShellInjectViewModel;

    private sealed class ConventionPopup : Popup;

    private sealed class ConventionPopupViewModel : ShellInjectViewModel;

    private sealed class ConstructorFailurePage : ContentPage;

    private sealed class ConstructorFailureViewModel(string value) : ShellInjectViewModel
    {
        public string Value { get; } = value;
    }
}
