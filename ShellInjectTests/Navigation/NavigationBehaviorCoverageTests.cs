using ShellInject;
using ShellInject.Navigation;

namespace ShellInjectTests.Navigation;

public class NavigationBehaviorCoverageTests
{
    private readonly object _parameter = new { Value = "coverage data" };

    [Fact]
    public async Task OnShellNavigatedAsync_WhenSenderIsNotShell_ShouldReturnWithoutThrowing()
    {
        var nav = new ShellInjectNavigation();
        var args = new ShellNavigatedEventArgs("/from", "/to", ShellNavigationSource.Push);

        var exception = await Record.ExceptionAsync(() => nav.OnShellNavigatedAsync(new object(), args));

        Assert.Null(exception);
    }

    [Fact]
    public async Task OnShellNavigatedAsync_WhenConventionBindingCreatesViewModel_ShouldRunLifecycleMethods()
    {
        var page = new ConventionLifecyclePage();
        var shell = CreateShellWithContent(new ShellContent { Content = page });
        var nav = new ShellInjectNavigation();
        var args = new ShellNavigatedEventArgs("/from", "/to", ShellNavigationSource.Push);

        await nav.OnShellNavigatedAsync(shell, args);

        var viewModel = Assert.IsType<ConventionLifecycleViewModel>(page.BindingContext);
        Assert.Equal(1, viewModel.AppearingCount);
        Assert.Equal(1, viewModel.AppearedCount);
        Assert.Equal(1, viewModel.InitializedCount);
    }

    [Fact]
    public async Task OnShellNavigatedAsync_WhenConventionBindingCreatesViewModelAndParameterExists_ShouldSendData()
    {
        var page = new ConventionLifecyclePage();
        var shell = CreateShellWithContent(new ShellContent { Content = page });
        var nav = new ShellInjectNavigation { NavigationParameter = _parameter };
        var args = new ShellNavigatedEventArgs("/from", "/to", ShellNavigationSource.Push);

        await nav.OnShellNavigatedAsync(shell, args);

        var viewModel = Assert.IsType<ConventionLifecycleViewModel>(page.BindingContext);
        Assert.Same(_parameter, viewModel.DataReceivedParameter);
    }

    [Fact]
    public async Task ChangeTabAsync_WhenTabIndexIsNegative_ShouldReturnWithoutThrowing()
    {
        var shell = CreateShellWithContent(new ShellContent { Content = new ContentPage() });
        var nav = new ShellInjectNavigation();

        var exception = await Record.ExceptionAsync(() => nav.ChangeTabAsync(shell, -1, _parameter, popToRootFirst: false));

        Assert.Null(exception);
    }

    [Fact]
    public async Task ChangeTabAsync_WhenNoShellSectionContainsTabIndex_ShouldReturnWithoutThrowing()
    {
        var shell = CreateShellWithContent(new ShellContent { Content = new ContentPage() });
        var nav = new ShellInjectNavigation();

        var exception = await Record.ExceptionAsync(() => nav.ChangeTabAsync(shell, 5, _parameter, popToRootFirst: false));

        Assert.Null(exception);
    }

    [Fact]
    public async Task PopModalStackAsync_WhenNoModalIsOpen_ShouldReturnWithoutThrowing()
    {
        var shell = CreateShellWithContent(new ShellContent { Content = new ContentPage() });
        var nav = new ShellInjectNavigation();

        var exception = await Record.ExceptionAsync(() => nav.PopModalStackAsync(shell, _parameter, animate: false));

        Assert.Null(exception);
    }

    [Fact]
    public async Task PushMultiStackAsync_WhenPageTypesIsNull_ShouldThrow()
    {
        var nav = new ShellInjectNavigation();

        await Assert.ThrowsAsync<NullReferenceException>(() => nav.PushMultiStackAsync(new Shell(), null!, _parameter, true, false));
    }

    [Fact]
    public async Task PushMultiStackAsync_WhenPageTypesIsEmpty_ShouldThrow()
    {
        var nav = new ShellInjectNavigation();

        await Assert.ThrowsAsync<NullReferenceException>(() => nav.PushMultiStackAsync(new Shell(), [], _parameter, true, false));
    }

    [Fact]
    public async Task ReplaceAsync_WhenPageTypeIsNull_ShouldReturnWithoutThrowing()
    {
        var shell = CreateShellWithContent(new ShellContent { Content = new ContentPage() });
        var nav = new ShellInjectNavigation();

        var exception = await Record.ExceptionAsync(() => nav.ReplaceAsync(shell, null, _parameter));

        Assert.Null(exception);
    }

    [Fact]
    public async Task SendDataToPageAsync_WhenPageTypeIsNull_ShouldReturnWithoutThrowing()
    {
        var shell = CreateShellWithContent(new ShellContent { Content = new ContentPage() });
        var nav = new ShellInjectNavigation();

        var exception = await Record.ExceptionAsync(() => nav.SendDataToPageAsync(shell, null, _parameter));

        Assert.Null(exception);
    }

    [Fact]
    public async Task SendDataToPageAsync_WhenTargetPageIsNotOnStack_ShouldReturnWithoutThrowing()
    {
        var shell = CreateShellWithContent(new ShellContent { Content = new ContentPage() });
        var nav = new ShellInjectNavigation();

        var exception = await Record.ExceptionAsync(() => nav.SendDataToPageAsync(shell, typeof(ConventionLifecyclePage), _parameter));

        Assert.Null(exception);
    }

    [Fact]
    public async Task SendDataToPageAsync_WhenTargetPageIsOnStack_ShouldSendReverseData()
    {
        var shell = CreateShellWithContent(new ShellContent { Content = new ContentPage() });
        var page = new ConventionLifecyclePage { BindingContext = new ConventionLifecycleViewModel() };
        await shell.Navigation.PushAsync(page, false);
        var nav = new ShellInjectNavigation();

        await nav.SendDataToPageAsync(shell, typeof(ConventionLifecyclePage), _parameter);

        var viewModel = Assert.IsType<ConventionLifecycleViewModel>(page.BindingContext);
        Assert.Same(_parameter, viewModel.ReverseDataReceivedParameter);
    }

    [Fact]
    public async Task PopToRootAsync_WhenCurrentPageHasViewModel_ShouldReturnDataToRootPage()
    {
        var page = new ConventionLifecyclePage();
        var shell = CreateShellWithContent(new ShellContent { Content = page });
        var nav = new ShellInjectNavigation();

        var exception = await Record.ExceptionAsync(() => nav.PopToRootAsync(shell, _parameter, animate: false));

        Assert.Null(exception);
        Assert.Null(nav.NavigationParameter);
        Assert.False(nav.IsReverseNavigation);
    }

    [Fact]
    public async Task ShowPopupAsync_WhenShellHasNoCurrentPage_ShouldReturnWithoutThrowing()
    {
        var nav = new ShellInjectNavigation();

        var exception = await Record.ExceptionAsync(() => nav.ShowPopupAsync<TestPopup>(new Shell(), _parameter));

        Assert.Null(exception);
    }

    [Fact]
    public async Task DismissPopupAsync_WhenPopupStackIsEmpty_ShouldReturnWithoutThrowing()
    {
        var shell = CreateShellWithContent(new ShellContent { Content = new ContentPage() });
        var nav = new ShellInjectNavigation();

        var exception = await Record.ExceptionAsync(() => nav.DismissPopupAsync<TestPopup>(shell, _parameter));

        Assert.Null(exception);
    }

    [Fact]
    public async Task PushModalWithNavigation_WhenPageIsNull_ShouldThrow()
    {
        var nav = new ShellInjectNavigation();

        await Assert.ThrowsAsync<NullReferenceException>(() => nav.PushModalWithNavigation<object>(new Shell(), null!, _parameter));
    }

    [Fact]
    public async Task PushModalAsync_WhenPageCanBeCreated_ShouldPushModalAndSendData()
    {
        var shell = CreateShellWithContent(new ShellContent { Content = new ContentPage() });
        var nav = new ShellInjectNavigation();

        await nav.PushModalAsync(shell, typeof(ConventionLifecyclePage), _parameter, animate: false);

        var modalPage = Assert.Single(shell.Navigation.ModalStack);
        var viewModel = Assert.IsType<ConventionLifecycleViewModel>(modalPage.BindingContext);
        Assert.Same(_parameter, viewModel.DataReceivedParameter);
    }

    [Fact]
    public async Task PushModalWithNavigation_WhenPageHasViewModel_ShouldPushModalNavigationAndSendData()
    {
        var shell = CreateShellWithContent(new ShellContent { Content = new ContentPage() });
        var page = new ConventionLifecyclePage();
        var nav = new ShellInjectNavigation();

        await nav.PushModalWithNavigation(shell, page, _parameter, animate: false);

        var modalPage = Assert.IsType<NavigationPage>(Assert.Single(shell.Navigation.ModalStack));
        Assert.Same(page, modalPage.RootPage);
        var viewModel = Assert.IsType<ConventionLifecycleViewModel>(page.BindingContext);
        Assert.Same(_parameter, viewModel.DataReceivedParameter);
    }

    [Fact]
    public async Task PopModalStackAsync_WhenModalStackExists_ShouldCloseModalAndReturnData()
    {
        var rootViewModel = new ConventionLifecycleViewModel();
        var shell = CreateShellWithContent(new ShellContent
        {
            Content = new ContentPage { BindingContext = rootViewModel }
        });
        await shell.Navigation.PushModalAsync(new NavigationPage(new ContentPage()), false);
        var nav = new ShellInjectNavigation();

        await nav.PopModalStackAsync(shell, _parameter, animate: false);

        Assert.Empty(shell.Navigation.ModalStack);
        Assert.Same(_parameter, rootViewModel.ReverseDataReceivedParameter);
    }

    [Fact]
    public async Task PopAsync_WhenModalPageExists_ShouldCloseModalAndReturnData()
    {
        var rootViewModel = new ConventionLifecycleViewModel();
        var shell = CreateShellWithContent(new ShellContent
        {
            Content = new ContentPage { BindingContext = rootViewModel }
        });
        await shell.Navigation.PushModalAsync(new ContentPage(), false);
        var nav = new ShellInjectNavigation();

        await nav.PopAsync(shell, _parameter, animate: false);

        Assert.Empty(shell.Navigation.ModalStack);
        Assert.Same(_parameter, rootViewModel.ReverseDataReceivedParameter);
    }

    [Fact]
    public async Task PopAsync_WhenModalNavigationStackHasMultiplePages_ShouldPopWithinModalAndReturnData()
    {
        var rootModalPage = new ContentPage { BindingContext = new ConventionLifecycleViewModel() };
        var modalNavigation = new NavigationPage(rootModalPage);
        await modalNavigation.Navigation.PushAsync(new ContentPage(), false);
        var shell = CreateShellWithContent(new ShellContent { Content = new ContentPage() });
        await shell.Navigation.PushModalAsync(modalNavigation, false);
        var nav = new ShellInjectNavigation();

        await nav.PopAsync(shell, _parameter, animate: false);

        Assert.Single(shell.Navigation.ModalStack);
        Assert.Single(modalNavigation.Navigation.NavigationStack);
        var viewModel = Assert.IsType<ConventionLifecycleViewModel>(rootModalPage.BindingContext);
        Assert.Same(_parameter, viewModel.ReverseDataReceivedParameter);
    }

    [Fact]
    public async Task PushMultiStackAsync_WhenPagesAreValid_ShouldNavigateAndCleanupState()
    {
        var shell = CreateShellWithContent(new ShellContent { Content = new ContentPage() });
        var nav = new ShellInjectNavigation();

        var exception = await Record.ExceptionAsync(() => nav.PushMultiStackAsync(
            shell,
            [typeof(FirstStackPage), typeof(SecondStackPage)],
            _parameter,
            animate: false,
            animateAllPages: false));

        Assert.Null(exception);
        Assert.Null(nav.NavigationParameter);
        Assert.Null(nav.Shell);
    }

    [Fact]
    public async Task ReplaceAsync_WhenTargetShellContentExists_ShouldNavigateAndCleanupState()
    {
        var targetContent = new ShellContent
        {
            Route = nameof(ReplaceTargetPage),
            Content = new ReplaceTargetPage()
        };
        var shell = CreateShellWithContent(targetContent);
        var nav = new ShellInjectNavigation();

        var exception = await Record.ExceptionAsync(() => nav.ReplaceAsync(shell, typeof(ReplaceTargetPage), _parameter, animate: false));

        Assert.Null(exception);
        Assert.Null(nav.NavigationParameter);
        Assert.Null(nav.Shell);
    }

    private static Shell CreateShellWithContent(ShellContent shellContent)
    {
        var shellSection = new ShellSection { CurrentItem = shellContent };
        shellSection.Items.Add(shellContent);
        var shellItem = new ShellItem { CurrentItem = shellSection };
        shellItem.Items.Add(shellSection);
        var shell = new Shell { CurrentItem = shellItem };
        shell.Items.Add(shellItem);

        return shell;
    }

    private sealed class ConventionLifecyclePage : ContentPage;

    private sealed class FirstStackPage : ContentPage;

    private sealed class SecondStackPage : ContentPage;

    private sealed class ReplaceTargetPage : ContentPage;

    private sealed class ConventionLifecycleViewModel : ShellInjectViewModel
    {
        public int AppearingCount { get; private set; }

        public int AppearedCount { get; private set; }

        public int InitializedCount { get; private set; }

        public object? DataReceivedParameter { get; private set; }

        public object? ReverseDataReceivedParameter { get; private set; }

        public override void OnAppearing()
        {
            AppearingCount++;
        }

        public override Task OnAppearedAsync()
        {
            AppearedCount++;
            return Task.CompletedTask;
        }

        public override Task InitializedAsync()
        {
            InitializedCount++;
            return Task.CompletedTask;
        }

        public override Task DataReceivedAsync(object? parameter)
        {
            DataReceivedParameter = parameter;
            return Task.CompletedTask;
        }

        public override Task ReverseDataReceivedAsync(object? parameter)
        {
            ReverseDataReceivedParameter = parameter;
            return Task.CompletedTask;
        }
    }

    private sealed class TestPopup : CommunityToolkit.Maui.Views.Popup;
}
