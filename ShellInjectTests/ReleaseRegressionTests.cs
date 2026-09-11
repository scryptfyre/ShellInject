using System.Reflection;
using CommunityToolkit.Maui.Views;
using Moq;
using ShellInject;
using ShellInject.Extensions;
using ShellInject.Interfaces;
using ShellInject.Navigation;
using ShellInject.Services;

namespace ShellInjectTests;

public class ReleaseRegressionTests : Navigation.BaseNavigationTests, IDisposable
{
    private readonly Action<Exception>? _handler = ShellInjectInitializer.Options.ErrorHandler;
    private readonly IServiceProvider? _provider = ShellInjectInitializer.ServiceProvider;
    private readonly Dictionary<Type, Type> _registrations = new(Registrations);

    private static Dictionary<Type, Type> Registrations => (Dictionary<Type, Type>)typeof(ShellInjectOptions)
        .GetField("_explicitViewModelMap", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(ShellInjectInitializer.Options)!;

    public void Dispose()
    {
        ShellInjectInitializer.Options.ErrorHandler = _handler;
        ShellInjectInitializer.ServiceProvider = _provider;
        Registrations.Clear();
        foreach (var mapping in _registrations) Registrations.Add(mapping.Key, mapping.Value);
        ShellInjectPageExtensions.ClearConventionResolutionCache();
    }

    [Fact]
    public void OriginalTabContractsStillExist()
    {
        var method = Assert.Single(typeof(ShellNavigation).GetMethods(), m => m.Name == "ChangeTabAsync" && !m.IsGenericMethod);
        Assert.Equal(new[] { typeof(Shell), typeof(int), typeof(object), typeof(bool) }, method.GetParameters().Select(p => p.ParameterType));
        Assert.Equal(4, typeof(IShellInjectNavigation).GetMethod("ChangeTabAsync")!.GetParameters().Length);
    }

    [Fact]
    public async Task LegacyInvalidArgumentsKeepExceptionTypes()
    {
        await Assert.ThrowsAsync<NullReferenceException>(() => ShellNavigation.PushModalWithNavigationAsync(shell: new Shell()));
        await Assert.ThrowsAsync<NullReferenceException>(() => ShellNavigation.PushMultiStackAsync(shell: new Shell()));
        await Assert.ThrowsAsync<InvalidOperationException>(() => ShellNavigation.PushAsync<ContentPage, string>("data"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => ShellNavigation.PushModalAsync<ContentPage, string>("data"));
    }

    [Fact]
    public void RegistrationOverridesCachedMissAndPreservesExistingContext()
    {
        ShellInjectInitializer.ServiceProvider = null;
        var page = new RegisteredReleasePage();
        Assert.False(ShellInjectPageExtensions.TryBindViewModelByConvention(page));
        Assert.False(ShellInjectPageExtensions.TryBindViewModelByConvention(new RegisteredReleasePage()));
        ShellInjectInitializer.Options.RegisterViewModel<RegisteredReleasePage, RecordingVm>();
        Assert.True(ShellInjectPageExtensions.TryBindViewModelByConvention(page));
        var context = Assert.IsType<RecordingVm>(page.BindingContext);
        Assert.False(ShellInjectPageExtensions.TryBindViewModelByConvention(page));
        Assert.Same(context, page.BindingContext);
        Assert.Null(ShellInjectPageExtensions.GetViewModelType(page));
    }

    [Fact]
    public void RegistrationValidatesNulls()
    {
        var options = new ShellInjectOptions();
        Assert.Throws<ArgumentNullException>(() => options.RegisterViewModel(null!, typeof(RecordingVm)));
        Assert.Throws<ArgumentNullException>(() => options.RegisterViewModel(typeof(ContentPage), null!));
    }

    [Fact]
    public void BindingFailuresAreReportedAndDiagnosticFailuresAreContained()
    {
        ShellInjectInitializer.ServiceProvider = null;
        var errors = new List<Exception>();
        ShellInjectInitializer.Options.ErrorHandler = errors.Add;
        var page = new ContentPage();
        ShellInjectPageExtensions.SetViewModelType(page, typeof(BrokenVm));
        Assert.Single(errors);
        Assert.Null(page.BindingContext);
        ShellInjectInitializer.Options.ErrorHandler = _ => throw new Exception("diagnostic failure");
        ShellInjectPageExtensions.SetViewModelType(new ContentPage(), typeof(BrokenVm));
    }

    [Fact]
    public async Task TypedHooksReceiveValuesNullsAndReportMismatches()
    {
        var errors = new List<Exception>();
        ShellInjectInitializer.Options.ErrorHandler = errors.Add;
        var vm = new StringVm();
        IShellInjectShellViewModel receiver = vm;
        await receiver.DataReceivedAsync("forward");
        await receiver.ReverseDataReceivedAsync("reverse");
        Assert.Equal("forward", vm.Forward);
        Assert.Equal("reverse", vm.Reverse);
        await receiver.DataReceivedAsync(null);
        await receiver.ReverseDataReceivedAsync(null);
        Assert.Null(vm.Forward);
        Assert.Null(vm.Reverse);
        await receiver.DataReceivedAsync(42);
        await receiver.ReverseDataReceivedAsync(42);
        Assert.Equal(2, errors.Count);
        Assert.All(errors, e => Assert.IsType<InvalidCastException>(e));
        receiver = new IntVm();
        await receiver.DataReceivedAsync(null);
        await receiver.ReverseDataReceivedAsync(null);
        Assert.Equal(4, errors.Count);
        var defaults = new DefaultVm();
        await defaults.DataReceivedAsync("value");
        await defaults.ReverseDataReceivedAsync("value");
    }

    [Fact]
    public void NewLifecycleSpellingWorksThroughLegacyCommand()
    {
        var vm = new NewLifecycleVm();
        vm.OnDisAppearingCommand.Execute(null);
        Assert.Equal(1, vm.Count);
    }

    [Fact]
    public async Task ExactPageTypeWinsOverEarlierSameNamePage()
    {
        SetupShell_Page_And_BindingContext();
        var first = new RecordingVm();
        var second = new RecordingVm();
        await TestShell.Navigation.PushAsync(new First.SharedPage { BindingContext = first }, false);
        await TestShell.Navigation.PushAsync(new Second.SharedPage { BindingContext = second }, false);
        await new ShellInjectNavigation().SendDataToPageAsync(TestShell, typeof(Second.SharedPage), "result");
        Assert.Null(first.Reverse);
        Assert.Equal("result", second.Reverse);
    }

    [Fact]
    public async Task LegacyNameFallbackIsPreserved()
    {
        SetupShell_Page_And_BindingContext();
        var vm = new RecordingVm();
        await TestShell.Navigation.PushAsync(new First.SharedPage { BindingContext = vm }, false);
        await new ShellInjectNavigation().SendDataToPageAsync(TestShell, typeof(Second.SharedPage), "result");
        Assert.Equal("result", vm.Reverse);
    }

    [Fact]
    public async Task TypedTabSelectionFindsMaterializedPageAndMissingTypeFallsBack()
    {
        SetupShell_Page_And_BindingContext();
        var content = new ShellContent { Content = new RegisteredReleasePage { BindingContext = new RecordingVm() } };
        var section = new ShellSection();
        section.Items.Add(content);
        section.CurrentItem = content;
        var item = new ShellItem();
        item.Items.Add(section);
        item.CurrentItem = section;
        TestShell.Items.Add(item);
        await ShellNavigation.ChangeTabAsync<RegisteredReleasePage>(TestShell, parameter: "tab", popToRootFirst: false);
        Assert.Same(item, TestShell.CurrentItem);
        Assert.Equal("tab", ((RecordingVm)((Page)content.Content).BindingContext).Forward);
        await ShellNavigation.ChangeTabAsync<First.SharedPage>(TestShell, parameter: "fallback", popToRootFirst: false);
        Assert.Equal("fallback", ((RecordingVm)((Page)content.Content).BindingContext).Forward);
    }

    [Fact]
    public void PopupTrackingIsIsolatedPerShell()
    {
        var method = typeof(ShellInjectNavigation).GetMethod("GetPopupStack", BindingFlags.NonPublic | BindingFlags.Static)!;
        var a = new Shell();
        var b = new Shell();
        Assert.Same(method.Invoke(null, [a]), method.Invoke(null, [a]));
        Assert.NotSame(method.Invoke(null, [a]), method.Invoke(null, [b]));
    }

    [Fact]
    public async Task DelayedLifecycleKeepsOriginalNavigationData()
    {
        SetupShell_Page_And_BindingContext();
        var vm = new DelayedVm();
        MockPage.BindingContext = vm;
        var nav = new ShellInjectNavigation { NavigationParameter = "original", IsReverseNavigation = true };
        var pending = nav.OnShellNavigatedAsync(TestShell, NavigatedEventArgsPush);
        nav.ShellTeardown(TestShell);
        nav.NavigationParameter = "next operation";
        vm.Ready.SetResult();
        await pending;
        Assert.Equal("original", vm.Reverse);
    }

    [Fact]
    public async Task PopupDismissalClosesOnlyOwningShellAndReturnsData()
    {
        SetupShell_Page_And_BindingContext();
        var vm = new RecordingVm();
        MockPage.BindingContext = vm;
        var popup = new TestPopup();
        var stack = PopupStack(TestShell);
        stack.Add(popup);
        var nav = new ShellInjectNavigation();
        await nav.DismissPopupAsync<TestPopup>(new Shell(), "wrong shell");
        Assert.Equal(0, popup.CloseCount);
        await nav.DismissPopupAsync<TestPopup>(TestShell, "closed");
        Assert.Equal(1, popup.CloseCount);
        Assert.Equal("closed", vm.Reverse);
        Assert.Empty(stack);
    }

    [Fact]
    public async Task DisposedPopupIsRemovedAndCallbackFailuresAreReported()
    {
        SetupShell_Page_And_BindingContext();
        var stack = PopupStack(TestShell);
        var nav = new ShellInjectNavigation();
        stack.Add(new TestPopup { Disposed = true });
        await nav.DismissPopupAsync<TestPopup>(TestShell, null);
        Assert.Empty(stack);
        var errors = new List<Exception>();
        ShellInjectInitializer.Options.ErrorHandler = errors.Add;
        MockPage.BindingContext = new ThrowingVm();
        stack.Add(new TestPopup());
        await nav.DismissPopupAsync<TestPopup>(TestShell, "result");
        Assert.Single(errors);
        Assert.Empty(stack);
    }

    private static List<Popup> PopupStack(Shell shell) => (List<Popup>)typeof(ShellInjectNavigation)
        .GetMethod("GetPopupStack", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [shell])!;

    [Fact]
    public async Task PopupShowAndCloseCompleteThroughToolkitModalStack()
    {
        SetupShell_Page_And_BindingContext();
        ShellInjectInitializer.ServiceProvider = null;
        var nav = new ShellInjectNavigation();
        var shown = nav.ShowPopupAsync<Popup>(TestShell, null);
        var popup = Assert.Single(PopupStack(TestShell));
        var modalPage = Assert.Single(TestShell.Navigation.ModalStack);
        // Native handlers normally attach the modal page to its owning navigation proxy.
        ((Microsoft.Maui.Controls.Internals.NavigationProxy)modalPage.Navigation).Inner = TestShell.Navigation;
        await nav.DismissPopupAsync<Popup>(TestShell, null).WaitAsync(TimeSpan.FromSeconds(5));
        await shown.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Empty(PopupStack(TestShell));
    }

    [Fact]
    public async Task PresentationFailureDetachesAndRemovesPopup()
    {
        SetupShell_Page_And_BindingContext();
        var navigation = new Mock<INavigation>();
        navigation.Setup(n => n.PushModalAsync(It.IsAny<Page>(), false)).ThrowsAsync(new InvalidOperationException("push failed"));
        ((Microsoft.Maui.Controls.Internals.NavigationProxy)MockPage.Navigation).Inner = navigation.Object;
        await Assert.ThrowsAsync<InvalidOperationException>(() => new ShellInjectNavigation().ShowPopupAsync<Popup>(TestShell, null));
        Assert.Empty(PopupStack(TestShell));
    }

    [Fact]
    public async Task EmptyNavigationStackHasNoDataRecipient()
    {
        var shell = new Shell();
        Assert.Null(shell.Navigation.NavigationStack);
        await new ShellInjectNavigation().SendDataToPageAsync(shell, typeof(ContentPage), "unused");
    }

    [Fact]
    public void BindingEdgeCasesPreserveExplicitContextAndSupportGlobalNamespace()
    {
        ShellInjectInitializer.ServiceProvider = null;
        var global = new GlobalReleasePage();
        Assert.True(ShellInjectPageExtensions.TryBindViewModelByConvention(global));
        Assert.IsType<GlobalReleaseViewModel>(global.BindingContext);
        var explicitPage = new ContentPage();
        ShellInjectPageExtensions.SetViewModelType(explicitPage, typeof(BrokenVm));
        Assert.False(ShellInjectPageExtensions.TryBindViewModelByConvention(explicitPage));
        Assert.False(ShellInjectPageExtensions.TryBindViewModelByConvention(new UnsupportedBindable()));
        var original = global.BindingContext;
        typeof(ShellInjectPageExtensions).GetMethod("BindViewModel", BindingFlags.Static | BindingFlags.NonPublic)!
            .Invoke(null, [global, typeof(RecordingVm), false]);
        Assert.Same(original, global.BindingContext);
    }

    [Fact]
    public void PartiallyLoadableAssembliesRetainUsableTypes()
    {
        var assembly = new Mock<Assembly>();
        assembly.Setup(a => a.GetTypes()).Throws(new ReflectionTypeLoadException([typeof(RecordingVm), null!], [new TypeLoadException()]));
        var result = (IEnumerable<Type>)typeof(ShellInjectPageExtensions).GetMethod("GetLoadableTypes", BindingFlags.Static | BindingFlags.NonPublic)!
            .Invoke(null, [assembly.Object])!;
        Assert.Equal(typeof(RecordingVm), Assert.Single(result));
    }

    private sealed class UnsupportedBindable : BindableObject;

    private sealed class TestPopup : Popup
    {
        public int CloseCount;
        public bool Disposed;
        public override Task CloseAsync(CancellationToken token = default)
        {
            CloseCount++;
            if (Disposed) throw new ObjectDisposedException(nameof(TestPopup));
            typeof(Popup).GetMethod("NotifyPopupIsClosed", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(this, null);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingVm : ShellInjectViewModel
    {
        public override Task ReverseDataReceivedAsync(object? parameter) => throw new InvalidOperationException("callback failed");
    }

    private sealed class DelayedVm : ShellInjectViewModel
    {
        public readonly TaskCompletionSource Ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public object? Reverse;
        public override Task OnAppearedAsync() => Ready.Task;
        public override Task ReverseDataReceivedAsync(object? parameter) { Reverse = parameter; return Task.CompletedTask; }
    }

    private sealed class RegisteredReleasePage : ContentPage;
    private sealed class BrokenVm(string dependency) { public string Dependency { get; } = dependency; }
    private sealed class RecordingVm : ShellInjectViewModel
    {
        public object? Forward;
        public object? Reverse;
        public override Task DataReceivedAsync(object? parameter) { Forward = parameter; return Task.CompletedTask; }
        public override Task ReverseDataReceivedAsync(object? parameter) { Reverse = parameter; return Task.CompletedTask; }
    }
    private sealed class StringVm : ShellInjectViewModel<string>
    {
        public string? Forward;
        public string? Reverse;
        public override Task DataReceivedAsync(string? parameter) { Forward = parameter; return Task.CompletedTask; }
        public override Task ReverseDataReceivedAsync(string? parameter) { Reverse = parameter; return Task.CompletedTask; }
    }
    private sealed class IntVm : ShellInjectViewModel<int>;
    private sealed class DefaultVm : ShellInjectViewModel<string>;
    private sealed class NewLifecycleVm : ShellInjectViewModel
    {
        public int Count;
        public override void OnDisappearing() => Count++;
    }
    private static class First { public sealed class SharedPage : ContentPage; }
    private static class Second { public sealed class SharedPage : ContentPage; }
}
