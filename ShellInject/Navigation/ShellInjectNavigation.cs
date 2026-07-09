using System.Collections.Concurrent;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.DependencyInjection;
using ShellInject.Constants;
using ShellInject.Extensions;
using ShellInject.Interfaces;

namespace ShellInject.Navigation;

/// <summary>
/// Provides navigation methods for Shell-based navigation in a Maui application.
/// </summary>
internal class ShellInjectNavigation : IShellInjectNavigation
{
    private static readonly object RouteLock = new();
    private static readonly ConcurrentDictionary<Type, string> RegisteredRoutes = new();
    private static readonly List<Popup> PopupStack = [];
    private static readonly object PopupStackLock = new();
    private readonly SemaphoreSlim _navigationLock = new(1, 1);
    
    /// <summary>
    /// Provides a singleton instance of the <see cref="ShellInjectNavigation"/> class for Shell-based navigation in a Maui application.
    /// </summary>
    internal static ShellInjectNavigation Instance { get; } = new();
    
    public virtual EventHandler<ShellNavigatedEventArgs>? NavigatedHandler { get; set; }

    /// <summary>
    /// Represents the parameter passed during navigation in a Shell-based Maui application.
    /// </summary>
    public virtual object? NavigationParameter { get; set; }

    /// <summary>
    /// Indicates whether the navigation is being performed in reverse.
    /// </summary>
    public virtual bool IsReverseNavigation { get; set; }

    /// <summary>
    /// Represents a Shell instance used for navigation in a Maui application.
    /// </summary>
    /// <remarks>
    /// The <see cref="Shell"/> variable holds a reference to the Shell instance, which is used for navigating between pages
    /// in a Shell-based navigation architecture. It is set during the Shell setup process, and should be null when the Shell is not available.
    /// </remarks>
    public virtual Shell? Shell { get; set; }

    /// <summary>
    /// Sets up the shell for navigation.
    /// </summary>
    /// <param name="shell">The Shell instance to be set up.</param>
    /// <param name="addNavigatedHandler"></param>
    /// <exception cref="NullReferenceException">Thrown if the given shell is null.</exception>
    private void ShellSetup(Shell shell, bool addNavigatedHandler = true)
    {
        if (shell == null)
        {
            throw new NullReferenceException(ShellInjectConstants.ShellNotFoundText);
        }

        if (addNavigatedHandler)
        {
            NavigatedHandler = async void (s, e) =>
            {
                try
                {
                    await OnShellNavigatedAsync(s, e);
                }
                catch
                {
                    // just catch it
                }
            };
            shell.Navigated += NavigatedHandler;
        }

        Shell = shell;
    }

    /// <summary>
    /// Teardown method for Shell navigation.
    /// </summary>
    /// <param name="shell">The Shell instance to teardown.</param>
    public virtual void ShellTeardown(Shell shell)
    {
        if (NavigatedHandler is not null)
        {
            shell.Navigated -= NavigatedHandler;
        }
        
        Shell = null;
        NavigationParameter = null;
        IsReverseNavigation = false;
        NavigatedHandler = null;
    }

    /// <summary>
    /// Sets the navigation parameter to be passed during navigation.
    /// </summary>
    /// <param name="parameter">The navigation parameter to be assigned. Can be null.</param>
    public virtual void SetNavigationParameter(object? parameter)
    {
        NavigationParameter = parameter;
    }

    private async Task RunSerializedNavigationAsync(Func<Task> navigationOperation)
    {
        await _navigationLock.WaitAsync();
        try
        {
            await navigationOperation();
        }
        finally
        {
            _navigationLock.Release();
        }
    }

    /// <summary>
    /// Builds a ShellInject-owned route name for the given page type without relying on MAUI internals.
    /// </summary>
    private static string BuildRouteName(Type pageType, bool includeNamespace = false)
    {
        var routeSource = includeNamespace
            ? pageType.FullName ?? pageType.Name
            : pageType.Name;

        return $"si_{SanitizeRouteName(routeSource)}";
    }

    private static string SanitizeRouteName(string routeSource)
    {
        return routeSource
            .Replace('.', '_')
            .Replace('+', '_')
            .Replace('`', '_');
    }

    /// <summary>
    /// Registers a route for a page type.
    /// </summary>
    /// <param name="pageType">The type of the page to register the route for.</param>
    public virtual string RegisterRoute(Type pageType)
    {
        ArgumentNullException.ThrowIfNull(pageType);

        if (!typeof(Page).IsAssignableFrom(pageType))
        {
            throw new ArgumentException($"Type must derive from {nameof(Page)}.", nameof(pageType));
        }

        lock (RouteLock)
        {
            if (RegisteredRoutes.TryGetValue(pageType, out var registeredRoute))
            {
                return registeredRoute;
            }

            var route = BuildRouteName(pageType);
            if (!TryRegisterRoute(route, pageType))
            {
                route = BuildRouteName(pageType, includeNamespace: true);
                if (!TryRegisterRoute(route, pageType))
                {
                    route = $"{route}_{Guid.NewGuid():N}";
                    Routing.RegisterRoute(route, pageType);
                }
            }

            RegisteredRoutes[pageType] = route;
            return route;
        }
    }

    private static bool TryRegisterRoute(string route, Type pageType)
    {
        try
        {
            Routing.RegisterRoute(route, pageType);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static object CreateInstance(Type type)
    {
        if (Injector.ServiceProvider is { } provider)
        {
            return ActivatorUtilities.GetServiceOrCreateInstance(provider, type);
        }

        return Activator.CreateInstance(type)
               ?? throw new InvalidOperationException($"Unable to create instance of type {type.FullName}.");
    }

    private static T CreateInstance<T>(Type type) where T : class
    {
        if (CreateInstance(type) is T instance)
        {
            return instance;
        }

        throw new ArgumentException($"Type must derive from {typeof(T).Name}.", nameof(type));
    }
    
    /// <summary>
    /// Handles navigation events triggered by the Shell after navigation has occurred.
    /// Updates the view model with navigation data or invokes relevant lifecycle methods.
    /// </summary>
    /// <param name="sender">The object that raised the event, typically the Shell instance.</param>
    /// <param name="e">The event data containing details about the navigation that occurred.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task OnShellNavigatedAsync(object? sender, ShellNavigatedEventArgs e)
    {
        if (sender is not Shell shell)
        {
            return;
        }

        await HandleShellNavigatedAsync(shell);
    }

    private async Task HandleShellNavigatedAsync(Shell shell)
    {
        var presentedPage = (shell.CurrentItem?.CurrentItem as IShellSectionController)?.PresentedPage;
        var page = presentedPage ?? shell.CurrentPage;
        var boundByConvention = TryBindViewModel(page);
        if (page is ContentPage { BindingContext: IShellInjectShellViewModel viewModel })
        {
            if (boundByConvention)
            {
                viewModel.OnAppearing();
            }

            await viewModel.OnAppearedAsync();

            if (!viewModel.IsInitialized)
            {
                await viewModel.InitializedAsync();
                viewModel.IsInitialized = true;
            }
            
            if (NavigationParameter is not null)
            {
                if (IsReverseNavigation)
                {
                    await viewModel.ReverseDataReceivedAsync(NavigationParameter);
                }
                else
                {
                    await viewModel.DataReceivedAsync(NavigationParameter);
                }
            }
        }
    }

    private static bool TryBindViewModel(BindableObject? bindable)
    {
        return bindable is not null && ShellInjectPageExtensions.TryBindViewModelByConvention(bindable);
    }

    /// <summary>
    /// Pushes a page onto the navigation stack.
    /// </summary>
    /// <param name="shell">The Shell instance to navigate on.</param>
    /// <param name="pageType">The type of the page to push.</param>
    /// <param name="tParameter">The parameter for the page.</param>
    /// <param name="animate">True to animate the transition, false otherwise. Default is true.</param>
    /// <returns>A Task representing the ongoing asynchronous operation.</returns>
    public virtual async Task PushAsync(Shell shell, Type pageType, object? tParameter, bool animate = true)
    {
        await RunSerializedNavigationAsync(async () =>
        {
            var route = RegisterRoute(pageType);
            ShellSetup(shell);
            try
            {
                SetNavigationParameter(tParameter);
                await shell.GoToAsync(route, animate);
            }
            finally
            {
                ShellTeardown(shell);
            }
        });
    }

    /// <summary>
    /// Resets the navigation and replaces the current main page
    /// </summary>
    /// <param name="shell"></param>
    /// <param name="pageType"></param>
    /// <param name="tParameter"></param>
    /// <param name="animate"></param>
    /// <typeparam name="TParameter"></typeparam>
    public virtual async Task ReplaceAsync<TParameter>(Shell shell, Type? pageType, TParameter? tParameter, bool animate = true)
    {
        if (pageType == null)
        {
            return;
        }

        await RunSerializedNavigationAsync(async () =>
        {
            ShellSetup(shell, false);
            try
            {
                SetNavigationParameter(tParameter);
                await shell.Navigation.PopToRootAsync(false);
                await shell.GoToAsync($"//{pageType.Name}", animate: animate);
                await HandleShellNavigatedAsync(shell);
            }
            finally
            {
                ShellTeardown(shell);
            }
        });
    }

    /// <summary>
    /// Changes the currently selected tab in a Shell-based Maui application.
    /// </summary>
    /// <typeparam name="TParameter">The type of the parameter passed during navigation.</typeparam>
    /// <param name="shell">The Shell instance.</param>
    /// <param name="tabIndex">The index of the tab to be selected.</param>
    /// <param name="tParameter">The parameter passed during navigation.</param>
    /// <param name="popToRootFirst">A flag indicating whether to pop to the root before changing the tab.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ChangeTabAsync<TParameter>(Shell shell, int tabIndex, TParameter? tParameter, bool popToRootFirst)
    {
        await RunSerializedNavigationAsync(async () =>
        {
            ShellSetup(shell);
            try
            {
                var target = ResolveTargetTab(shell, tabIndex);
                if (target is null)
                {
                    return;
                }

                if (popToRootFirst && shell.Navigation?.NavigationStack?.Count > 1)
                {
                    await shell.Navigation.PopToRootAsync(false);
                }

                shell.CurrentItem = target.Value.ShellItem;
                target.Value.ShellItem.CurrentItem = target.Value.ShellSection;
                target.Value.ShellSection.CurrentItem = target.Value.ShellContent;

                var targetPage = ResolveShellContentPage(target.Value.ShellContent)
                                 ?? (target.Value.ShellSection as IShellSectionController)?.PresentedPage
                                 ?? shell.CurrentPage;
                var boundByConvention = TryBindViewModel(targetPage);
                if (targetPage?.BindingContext is IShellInjectShellViewModel vm)
                {
                    if (boundByConvention)
                    {
                        vm.OnAppearing();
                    }

                    await vm.DataReceivedAsync(tParameter);
                }
            }
            finally
            {
                ShellTeardown(shell);
            }
        });
    }

    private static (ShellItem ShellItem, ShellSection ShellSection, ShellContent ShellContent)? ResolveTargetTab(Shell shell, int tabIndex)
    {
        if (tabIndex < 0)
        {
            return null;
        }

        var currentShellItem = shell.CurrentItem;
        var currentShellSection = currentShellItem?.CurrentItem;
        if (currentShellItem is not null && currentShellSection is not null && currentShellSection.Items.Count > tabIndex)
        {
            return (currentShellItem, currentShellSection, currentShellSection.Items[tabIndex]);
        }

        foreach (var shellItem in shell.Items)
        {
            foreach (var shellSection in shellItem.Items)
            {
                if (shellSection.Items.Count > tabIndex)
                {
                    return (shellItem, shellSection, shellSection.Items[tabIndex]);
                }
            }
        }

        return null;
    }

    private static Page? ResolveShellContentPage(ShellContent shellContent)
    {
        if (shellContent.Content is Page page)
        {
            return page;
        }

        if (shellContent.ContentTemplate?.CreateContent() is not Page templatedPage)
        {
            return null;
        }

        shellContent.Content = templatedPage;
        return templatedPage;
    }

    /// <summary>
    /// Asynchronously pops the topmost page from the navigation stack and returns a result value.
    /// </summary>
    /// <typeparam name="TResult">The type of the result value.</typeparam>
    /// <param name="shell">The Shell instance to perform the pop operation on.</param>
    /// <param name="tResult">The result value.</param>
    /// <param name="animate">Whether to animate the pop transition. Default is true.</param>
    /// <returns>A task representing the asynchronous pop operation.</returns>
    public async Task PopAsync<TResult>(Shell shell, TResult tResult, bool animate = true)
    {
        await RunSerializedNavigationAsync(async () =>
        {
            if (shell.Navigation.ModalStack.Count > 0)
            {
                await PopCurrentModalAsync(shell, tResult, animate);
                return;
            }

            ShellSetup(shell);
            try
            {
                IsReverseNavigation = true;
                SetNavigationParameter(tResult);
                await shell.GoToAsync("..", animate);
            }
            finally
            {
                ShellTeardown(shell);
            }
        });
    }

    private static async Task PopCurrentModalAsync<TResult>(Shell shell, TResult tResult, bool animate)
    {
        var modalPage = shell.Navigation.ModalStack[^1];
        var modalNavigation = modalPage.Navigation;
        if (modalNavigation.NavigationStack.Count > 1)
        {
            await modalNavigation.PopAsync(animate);
            var targetModalPage = modalNavigation.NavigationStack.LastOrDefault();
            TryBindViewModel(targetModalPage);
            if (targetModalPage?.BindingContext is IShellInjectShellViewModel modalVm)
            {
                await modalVm.ReverseDataReceivedAsync(tResult);
            }

            return;
        }

        await shell.Navigation.PopModalAsync(animate);
        var currentPage = shell.CurrentPage;
        TryBindViewModel(currentPage);
        if (currentPage?.BindingContext is IShellInjectShellViewModel vm)
        {
            await vm.ReverseDataReceivedAsync(tResult);
        }
    }

    /// <summary>
    /// Closes the entire current Modal stack with Optional Parameter
    /// </summary>
    /// <param name="shell"></param>
    /// <param name="data"></param>
    /// <param name="animate"></param>
    /// <returns></returns>
    public async Task PopModalStackAsync(Shell shell, object? data, bool animate)
    {
        await RunSerializedNavigationAsync(async () =>
        {
            var modalStack = shell.Navigation.ModalStack;
            if (modalStack.Count == 0)
            {
                return;
            }

            var modalPage = modalStack[^1];
            var modalNavigation = modalPage.Navigation;
            var navigationStack = modalNavigation.NavigationStack;
            while (navigationStack.Count > 1)
            {
                await modalNavigation.PopAsync(false);
            }

            await shell.Navigation.PopModalAsync(animate);

            var currentPage = shell.CurrentPage;
            TryBindViewModel(currentPage);
            if (currentPage?.BindingContext is IShellInjectShellViewModel vm)
            {
                await vm.ReverseDataReceivedAsync(data);
            }
        });
    }

    /// <summary>
    /// Navigates back to the specified page in the navigation stack.
    /// If the specified page is not found, navigates back to the root.
    /// </summary>
    /// <typeparam name="TResult">The type of the data to be passed to the target page or handled after navigation.</typeparam>
    /// <param name="shell">The Shell instance used for navigation.</param>
    /// <param name="pageType">The type of the page to navigate back to.</param>
    /// <param name="tResult">The result or data to be passed during navigation.</param>
    /// <exception cref="ArgumentNullException">Thrown if the shell or pageType is null.</exception>
    public async Task PopToAsync<TResult>(Shell shell, Type pageType, TResult tResult)
    {
        await RunSerializedNavigationAsync(async () =>
        {
            if (shell.CurrentPage?.GetType() == pageType)
            {
                TryBindViewModel(shell.CurrentPage);
                if (shell.CurrentPage.BindingContext is IShellInjectShellViewModel vm)
                {
                    await vm.ReverseDataReceivedAsync(tResult);
                }

                return;
            }

            var navigationStack = shell.Navigation.NavigationStack;
            if (navigationStack is { Count: > 0 })
            {
                // Iterate backwards through the navigation stack to find the target page type
                for (int i = navigationStack.Count - 1; i >= 0; i--)
                {
                    var item = navigationStack[i];

                    // Skip null entries
                    if (item != null && item.GetType() == pageType)
                    {
                        TryBindViewModel(item);

                        // Calculate how many pages to pop back (relative navigation)
                        var pagesToPop = navigationStack.Count - 1 - i;

                        // Pop the required number of pages back using relative routing
                        if (pagesToPop == 0)
                        {
                            if (item.BindingContext is IShellInjectShellViewModel vm)
                            {
                                await vm.ReverseDataReceivedAsync(tResult);
                            }

                            return;
                        }

                        for (int popCount = 0; popCount < pagesToPop; popCount++)
                        {
                            if (popCount == pagesToPop - 1)
                            {
                                ShellSetup(shell);
                                try
                                {
                                    IsReverseNavigation = true;
                                    SetNavigationParameter(tResult);
                                    await shell.GoToAsync("..", true);
                                }
                                finally
                                {
                                    ShellTeardown(shell);
                                }

                                continue;
                            }

                            await shell.GoToAsync("..", false);
                        }

                        return;
                    }
                }
            }

            // If pageType not found, return to root
            await shell.GoToAsync("//", true);
        });
    }

    /// <summary>
    /// Pops all pages from the navigation stack and returns to the root page.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="shell">The Shell instance to perform the navigation.</param>
    /// <param name="tResult">The result parameter to be passed during navigation.</param>
    /// <param name="animate">True to animate the navigation, false otherwise. Default is true.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task PopToRootAsync<TResult>(Shell shell, TResult tResult, bool animate = true)
    {
        await RunSerializedNavigationAsync(async () =>
        {
            ShellSetup(shell);
            try
            {
                IsReverseNavigation = true;
                SetNavigationParameter(tResult);
                await shell.Navigation.PopToRootAsync(animate);
            }
            finally
            {
                ShellTeardown(shell);
            }
        });
    }

    /// <summary>
    /// Pushes multiple pages onto the Shell navigation stack asynchronously.
    /// </summary>
    /// <typeparam name="TParameter">The type of the parameter to pass to the pages.</typeparam>
    /// <param name="shell">The Shell instance.</param>
    /// <param name="pageTypes">The list of page types to navigate to.</param>
    /// <param name="tParameter">The parameter to pass to the pages.</param>
    /// <param name="animate">A boolean value indicating whether to animate the navigation.</param>
    /// <param name="animateAllPages">A boolean value indicating whether to animate all pages during the navigation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="NullReferenceException">Thrown when the pageTypes parameter is null or empty.</exception>
    public async Task PushMultiStackAsync<TParameter>(Shell shell, List<Type> pageTypes, TParameter tParameter, bool animate, bool animateAllPages)
    {
        if (pageTypes == null || pageTypes.Count == 0)
        {
            throw new NullReferenceException(ShellInjectConstants.NavigationStatesExceptionText);
        }

        await RunSerializedNavigationAsync(async () =>
        {
            var lastState = pageTypes.Last();
            try
            {
                foreach (var type in pageTypes)
                {
                    var route = RegisterRoute(type);
                    if (type == lastState)
                    {
                        ShellSetup(shell);
                        SetNavigationParameter(tParameter);
                    }

                    await shell.GoToAsync(route, type == lastState ? animate : animateAllPages);
                }
            }
            finally
            {
                ShellTeardown(shell);
            }
        });
    }

    /// <summary>
    /// Pushes a ContentPage as a modal with navigation.
    /// </summary>
    /// <typeparam name="TParameter">The type of the parameter to be passed to the view model associated with the page. Pass null if no parameter needs to be passed.</typeparam>
    /// <param name="shell">The Shell instance.</param>
    /// <param name="page">The ContentPage to be pushed.</param>
    /// <param name="tParameter">The parameter to be passed to the view model associated with the page.</param>
    /// <param name="animate">Specifies whether the navigation transition should be animated or not. Default value is true.</param>
    /// <exception cref="NullReferenceException">Thrown if the given shell is null or the given page is null.</exception>
    public async Task PushModalWithNavigation<TParameter>(Shell shell, ContentPage page, TParameter? tParameter, bool animate = true)
    {
        if (page == null)
        {
            throw new NullReferenceException(ShellInjectConstants.NullContentPageExceptionText);
        }
        
        await RunSerializedNavigationAsync(async () =>
        {
            TryBindViewModel(page);
            ShellSetup(shell);
            try
            {
                await shell.Navigation.PushModalAsync(new NavigationPage(page), animate);
                if (page.BindingContext is IShellInjectShellViewModel vm && tParameter != null)
                {
                    await vm.DataReceivedAsync(tParameter);
                }
            }
            finally
            {
                ShellTeardown(shell);
            }
        });
    }

    /// <summary>
    /// Pushes a ContentPage as a modal
    /// </summary>
    /// <param name="shell"></param>
    /// <param name="pageType"></param>
    /// <param name="tParameter"></param>
    /// <param name="animate"></param>
    /// <exception cref="NullReferenceException"></exception>
    public async Task PushModalAsync(Shell shell, Type pageType, object? tParameter, bool animate = true)
    {
        await RunSerializedNavigationAsync(async () =>
        {
            var contentPage = CreateInstance<ContentPage>(pageType);
            TryBindViewModel(contentPage);
            ShellSetup(shell);
            try
            {
                await shell.Navigation.PushModalAsync(contentPage, animate);
                if (contentPage.BindingContext is IShellInjectShellViewModel vm && tParameter != null)
                {
                    await vm.DataReceivedAsync(tParameter);
                }
            }
            finally
            {
                ShellTeardown(shell);
            }
        });
    }

    /// <summary>
    /// Looks for the specified Page on the stack and sends the data using the ReverseDataReceivedAsync method
    /// </summary>
    /// <param name="shell"></param>
    /// <param name="page"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public async Task SendDataToPageAsync(Shell shell, Type? page, object? data = null)
    {
        if (page == null || shell.Navigation == null)
        {
            return;
        }
    
        var navigationStack = shell.Navigation.NavigationStack;
        if (navigationStack == null || navigationStack.Count == 0)
        {
            return;
        }
    
        var pageToSendDataTo = navigationStack
            .Where(p => p != null)
            .FirstOrDefault(p => p.GetType().Name == page.Name);

        TryBindViewModel(pageToSendDataTo);
    
        if (pageToSendDataTo is { BindingContext: IShellInjectShellViewModel vm })
        {
            await vm.ReverseDataReceivedAsync(data);
        }
    }

    /// <summary>
    /// Shows/Creates a Popup of the Specified Type and passes in a data object
    /// </summary>
    /// <param name="shell"></param>
    /// <param name="data"></param>
    /// <param name="onError"></param>
    /// <typeparam name="TPopup"></typeparam>
    /// <returns></returns>
    public async Task ShowPopupAsync<TPopup>(Shell shell, object? data, Action<Exception>? onError = null) where TPopup : Popup
    {
        if (shell.CurrentPage is null)
        {
            return;
        }

        var popupPage = CreateInstance<Popup>(typeof(TPopup));
        TryBindViewModel(popupPage);

        lock (PopupStackLock)
        {
            PopupStack.Add(popupPage);
        }
        popupPage.Closed += OnPopupClosed;

        void OnPopupClosed(object? sender, EventArgs e)
        {
            popupPage.Closed -= OnPopupClosed;
            lock (PopupStackLock)
            {
                PopupStack.Remove(popupPage);
            }
        }

        async void OnPopupOpened(object? sender, EventArgs e)
        {
            popupPage.Opened -= OnPopupOpened;
            try
            {
                if (popupPage.BindingContext is IShellInjectShellViewModel vm)
                {
                    await vm.DataReceivedAsync(data);
                }
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
            }
        }

        popupPage.Opened += OnPopupOpened;
        try
        {
            await shell.CurrentPage.ShowPopupAsync(popupPage);
        }
        catch
        {
            popupPage.Opened -= OnPopupOpened;
            popupPage.Closed -= OnPopupClosed;
            lock (PopupStackLock)
            {
                PopupStack.Remove(popupPage);
            }

            throw;
        }
    }

    /// <summary>
    /// Dismisses popup of the specified Type with Parameters
    /// </summary>
    /// <param name="shell"></param>
    /// <param name="data"></param>
    /// <typeparam name="TPopup"></typeparam>
    public async Task DismissPopupAsync<TPopup>(Shell shell, object? data) where TPopup : Popup
    {
        List<TPopup> typedPopups;
        lock (PopupStackLock)
        {
            typedPopups = PopupStack.OfType<TPopup>().ToList();
        }

        if (typedPopups.Count == 0)
        {
            return;
        }
        
        var latestPopup = typedPopups.Last();
        
        try
        {
            if (data is not null)
            {
                EventHandler? handler = null;
                handler = async (sender, args) =>
                {
                    try
                    {
                        latestPopup.Closed -= handler;
                        var currentPage = shell.CurrentPage;
                        TryBindViewModel(currentPage);
                        if (currentPage?.BindingContext is IShellInjectShellViewModel vm)
                        {
                            await vm.ReverseDataReceivedAsync(data);
                        }
                    }
                    catch (Exception)
                    {
                        // just catch it
                    }
                };
                    
                latestPopup.Closed += handler;
            }
                
            await latestPopup.CloseAsync();
        }
        catch (ObjectDisposedException)
        {
            // Popup is already disposed, safe to ignore
        }
        finally
        {
            lock (PopupStackLock)
            {
                PopupStack.Remove(latestPopup);
            }
        }
    }
}
