using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using ShellInject.Constants;
using ShellInject.Navigation;

namespace ShellInject;

/// <summary>
/// Provides a static navigation entry point for Shell-based navigation.
/// </summary>
public static class ShellNavigation
{
    /// <summary>
    /// Pushes a page of the specified type onto the navigation stack asynchronously.
    /// </summary>
    /// <typeparam name="TPageType">The type of the page to be pushed. Must be a subclass of ContentPage.</typeparam>
    /// <param name="shell">The Shell instance used for navigation. Defaults to Shell.Current.</param>
    /// <param name="parameter">An optional parameter to pass to the pushed page. Default is null.</param>
    /// <param name="animate">A boolean value indicating whether to animate the push operation. Default is true.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public static Task PushAsync<TPageType>(Shell? shell = null, object? parameter = null, bool animate = true)
        where TPageType : ContentPage
    {
        return ShellInjectNavigation.Instance.PushAsync(GetShellOrThrow(shell), typeof(TPageType), parameter, animate);
    }

    /// <summary>
    /// Replaces the current page in the navigation stack with a new page of the specified type asynchronously.
    /// Typically used when there is a Flyout menu and wanting to replace the main content.
    /// </summary>
    /// <typeparam name="TPageType">The type of the page to be replaced with. Must be a subclass of ContentPage.</typeparam>
    /// <param name="shell">The Shell instance used for navigation. Defaults to Shell.Current.</param>
    /// <param name="parameter">An optional parameter to pass to the new page. Default is null.</param>
    /// <param name="animate">A boolean value indicating whether to animate the replacement operation. Default is true.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public static Task ReplaceAsync<TPageType>(Shell? shell = null, object? parameter = null, bool animate = true)
        where TPageType : ContentPage
    {
        return ShellInjectNavigation.Instance.ReplaceAsync(GetShellOrThrow(shell), typeof(TPageType), parameter, animate);
    }

    /// <summary>
    /// Pops the current page from the navigation stack, returning to the previous page.
    /// </summary>
    /// <param name="shell">The Shell instance. Defaults to Shell.Current.</param>
    /// <param name="parameter">The optional parameter to pass to the previous page.</param>
    /// <param name="animate">Whether to animate the navigation transition. The default is true.</param>
    /// <returns>A task that represents the asynchronous operation of popping the page. The task completes when the navigation is finished.</returns>
    public static Task PopAsync(Shell? shell = null, object? parameter = null, bool animate = true)
    {
        return ShellInjectNavigation.Instance.PopAsync(GetShellOrThrow(shell), parameter, animate);
    }

    /// <summary>
    /// Closes the entire current Modal stack with Optional Parameter.
    /// </summary>
    /// <param name="shell">The Shell instance. Defaults to Shell.Current.</param>
    /// <param name="data">The optional data to pass back.</param>
    /// <param name="animate">Whether to animate the navigation transition. The default is true.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static Task PopModalStackAsync(Shell? shell = null, object? data = null, bool animate = true)
    {
        return ShellInjectNavigation.Instance.PopModalStackAsync(GetShellOrThrow(shell), data, animate);
    }

    /// <summary>
    /// Pops pages from the navigation stack until the specified page type is reached.
    /// </summary>
    /// <typeparam name="TPageType">The type of the target page to pop to. Must be a subclass of ContentPage.</typeparam>
    /// <param name="shell">The Shell instance used for navigation. Defaults to Shell.Current.</param>
    /// <param name="parameter">An optional parameter to pass to the target page. Default is null.</param>
    /// <returns>A Task representing the asynchronous pop operation.</returns>
    public static Task PopToAsync<TPageType>(Shell? shell = null, object? parameter = null)
    {
        return ShellInjectNavigation.Instance.PopToAsync(GetShellOrThrow(shell), typeof(TPageType), parameter);
    }

    /// <summary>
    /// Pops all pages from the navigation stack and returns to the root page.
    /// </summary>
    /// <param name="shell">The Shell instance to perform the navigation. Defaults to Shell.Current.</param>
    /// <param name="parameter">The optional parameter to pass to the root page.</param>
    /// <param name="animate">True to animate the navigation, false otherwise. Default is true.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static Task PopToRootAsync(Shell? shell = null, object? parameter = null, bool animate = true)
    {
        return ShellInjectNavigation.Instance.PopToRootAsync(GetShellOrThrow(shell), parameter, animate);
    }

    /// <summary>
    /// Changes the active tab of the Shell and navigates to the specified tab index asynchronously.
    /// </summary>
    /// <param name="shell">The Shell instance. Defaults to Shell.Current.</param>
    /// <param name="tabIndex">The index of the tab to navigate to.</param>
    /// <param name="parameter">An optional parameter to pass to the tab navigation.</param>
    /// <param name="popToRootFirst">A flag indicating whether to pop to the root of the navigation stack before changing the tab.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task ChangeTabAsync(Shell? shell = null, int tabIndex = 0, object? parameter = null, bool popToRootFirst = true)
    {
        return ShellInjectNavigation.Instance.ChangeTabAsync(GetShellOrThrow(shell), tabIndex, parameter, popToRootFirst);
    }

    /// <summary>
    /// Pushes multiple navigation stacks onto the Shell using a list of page types.
    /// </summary>
    /// <param name="shell">The Shell instance. Defaults to Shell.Current.</param>
    /// <param name="pageTypes">A list of page types to push onto the Shell.</param>
    /// <param name="parameter">The optional parameter to pass to the pages.</param>
    /// <param name="animate">A boolean value indicating whether to animate the page transitions. Default is true.</param>
    /// <param name="animateAllPages">A boolean value indicating whether to animate all pages in the navigation stack. Default is false.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public static Task PushMultiStackAsync(Shell? shell = null, List<Type>? pageTypes = null, object? parameter = null, bool animate = true, bool animateAllPages = false)
    {
        return ShellInjectNavigation.Instance.PushMultiStackAsync(GetShellOrThrow(shell), pageTypes ?? [], parameter, animate, animateAllPages);
    }

    /// <summary>
    /// Pushes a modal page onto the navigation stack.
    /// </summary>
    /// <param name="shell">The Shell instance. Defaults to Shell.Current.</param>
    /// <param name="page">The modal page to push.</param>
    /// <param name="parameter">An optional parameter to pass to the page.</param>
    /// <param name="animate">A flag indicating whether to animate the transition. The default value is true.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public static Task PushModalWithNavigationAsync(Shell? shell = null, ContentPage? page = null, object? parameter = null, bool animate = true)
    {
        return ShellInjectNavigation.Instance.PushModalWithNavigation(GetShellOrThrow(shell), page ?? throw new NullReferenceException(ShellInjectConstants.NullContentPageExceptionText), parameter, animate);
    }

    /// <summary>
    /// Pushes a modal page of the specified type onto the navigation stack asynchronously.
    /// </summary>
    /// <typeparam name="TPageType">The type of the modal page to be pushed. Must be a subclass of ContentPage.</typeparam>
    /// <param name="shell">The Shell instance used for navigation. Defaults to Shell.Current.</param>
    /// <param name="parameter">An optional parameter to pass to the pushed page. Default is null.</param>
    /// <param name="animate">A boolean value indicating whether to animate the push operation. Default is true.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public static Task PushModalAsync<TPageType>(Shell? shell = null, object? parameter = null, bool animate = true)
        where TPageType : ContentPage
    {
        return ShellInjectNavigation.Instance.PushModalAsync(GetShellOrThrow(shell), typeof(TPageType), parameter, animate);
    }

    /// <summary>
    /// Sends data to a specified page of the given type on the navigation stack using the ReverseDataReceivedAsync method.
    /// </summary>
    /// <typeparam name="TPageType">The type of the page to which the data should be sent. Must be a subclass of ContentPage.</typeparam>
    /// <param name="shell">The Shell instance used to locate the target page. Defaults to Shell.Current.</param>
    /// <param name="data">The data to be sent to the target page. Default is null.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public static Task SendDataToPageAsync<TPageType>(Shell? shell = null, object? data = null)
        where TPageType : ContentPage
    {
        return ShellInjectNavigation.Instance.SendDataToPageAsync(GetShellOrThrow(shell), typeof(TPageType), data);
    }

    /// <summary>
    /// Shows/Creates a Popup of the Specified Type and passes in a data object.
    /// </summary>
    /// <param name="shell">The Shell instance. Defaults to Shell.Current.</param>
    /// <param name="data">The optional data to pass to the popup.</param>
    /// <param name="onError">Optional error handler.</param>
    /// <typeparam name="TPopup">The popup type.</typeparam>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public static Task ShowPopupAsync<TPopup>(Shell? shell = null, object? data = null, Action<Exception>? onError = null) where TPopup : Popup
    {
        return ShellInjectNavigation.Instance.ShowPopupAsync<TPopup>(GetShellOrThrow(shell), data, onError);
    }

    /// <summary>
    /// Dismisses popup of the specified Type with Parameters.
    /// </summary>
    /// <param name="shell">The Shell instance. Defaults to Shell.Current.</param>
    /// <param name="data">The optional data to pass back.</param>
    /// <typeparam name="TPopup">The popup type.</typeparam>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public static Task DismissPopupAsync<TPopup>(Shell? shell = null, object? data = null) where TPopup : Popup
    {
        return ShellInjectNavigation.Instance.DismissPopupAsync<TPopup>(GetShellOrThrow(shell), data);
    }

    private static Shell GetShellOrThrow(Shell? shell)
    {
        var resolvedShell = shell ?? Shell.Current;
        if (resolvedShell is null)
        {
            throw new InvalidOperationException(ShellInjectConstants.ShellNotFoundText);
        }

        return resolvedShell;
    }
}
