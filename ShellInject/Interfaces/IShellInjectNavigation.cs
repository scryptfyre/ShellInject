using CommunityToolkit.Maui.Views;

namespace ShellInject.Interfaces;

/// <summary>
/// Defines the navigation operations ShellInject performs against a <see cref="Shell"/>.
/// Application code normally calls the static <see cref="ShellNavigation"/> class instead of this interface.
/// </summary>
public interface IShellInjectNavigation
{
    /// <summary>
    /// Gets or sets the handler attached to <see cref="Shell.Navigated"/> for the current operation.
    /// </summary>
    EventHandler<ShellNavigatedEventArgs>? NavigatedHandler { get; set; }

    /// <summary>
    /// Gets or sets the data delivered to the destination ViewModel once the current operation completes.
    /// </summary>
    object? NavigationParameter { get; set; }

    /// <summary>
    /// Registers a global Shell route for the given page type, if one is not already registered.
    /// </summary>
    /// <param name="pageType">The page type to register.</param>
    /// <returns>The route name used for the page type.</returns>
    string RegisterRoute(Type pageType);

    /// <summary>
    /// Sets the data delivered to the destination ViewModel for the current operation.
    /// </summary>
    /// <param name="parameter">The data to deliver, or null to deliver nothing.</param>
    void SetNavigationParameter(object? parameter);

    /// <summary>
    /// Gets or sets a value indicating whether pending data is delivered to
    /// <see cref="IShellInjectShellViewModel.ReverseDataReceivedAsync"/> rather than
    /// <see cref="IShellInjectShellViewModel.DataReceivedAsync"/>.
    /// </summary>
    bool IsReverseNavigation { get; set; }

    /// <summary>
    /// Gets or sets the Shell targeted by the current operation.
    /// </summary>
    Shell? Shell { get; set; }

    /// <summary>
    /// Detaches the navigation handler and clears the state held for an operation.
    /// </summary>
    /// <param name="shell">The Shell to detach from.</param>
    void ShellTeardown(Shell shell);

    /// <summary>
    /// Handles <see cref="Shell.Navigated"/> by binding the destination ViewModel and delivering
    /// navigation data and lifecycle callbacks.
    /// </summary>
    /// <param name="sender">The Shell that raised the event.</param>
    /// <param name="e">The event data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task OnShellNavigatedAsync(object? sender, ShellNavigatedEventArgs e);

    /// <summary>
    /// Pushes a page onto the Shell navigation stack.
    /// </summary>
    /// <param name="shell">The Shell to navigate.</param>
    /// <param name="pageType">The type of page to push.</param>
    /// <param name="tParameter">Data delivered to the pushed page's ViewModel.</param>
    /// <param name="animate">Whether the transition is animated.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PushAsync(Shell shell, Type pageType, object? tParameter, bool animate = true);

    /// <summary>
    /// Navigates to a page that already exists in the Shell visual hierarchy, such as a flyout item.
    /// </summary>
    /// <typeparam name="TParameter">The type of data to deliver.</typeparam>
    /// <param name="shell">The Shell to navigate.</param>
    /// <param name="pageType">The destination page type, matched by its route name.</param>
    /// <param name="tParameter">Data delivered to the destination ViewModel.</param>
    /// <param name="animate">Whether the transition is animated.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ReplaceAsync<TParameter>(Shell shell, Type? pageType, TParameter? tParameter, bool animate = true);

    /// <summary>
    /// Selects a tab by index and delivers data to the selected tab's ViewModel.
    /// </summary>
    /// <typeparam name="TParameter">The type of data to deliver.</typeparam>
    /// <param name="shell">The Shell to navigate.</param>
    /// <param name="tabIndex">The zero-based index of the tab to select.</param>
    /// <param name="tParameter">Data delivered to the selected tab's ViewModel.</param>
    /// <param name="popToRootFirst">Whether the current stack is popped to its root before selecting the tab.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ChangeTabAsync<TParameter>(Shell shell, int tabIndex, TParameter? tParameter, bool popToRootFirst);

    /// <summary>
    /// Pops the current page and returns data to the previous page's ViewModel.
    /// Works for both Shell pages and modal pages.
    /// </summary>
    /// <typeparam name="TResult">The type of data to return.</typeparam>
    /// <param name="shell">The Shell to navigate.</param>
    /// <param name="tResult">Data delivered to the previous page's ViewModel.</param>
    /// <param name="animate">Whether the transition is animated.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PopAsync<TResult>(Shell shell, TResult tResult, bool animate = true);

    /// <summary>
    /// Closes an entire modal navigation stack and returns data to the page beneath it.
    /// </summary>
    /// <param name="shell">The Shell to navigate.</param>
    /// <param name="data">Data delivered to the revealed page's ViewModel.</param>
    /// <param name="animate">Whether the transition is animated.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PopModalStackAsync(Shell shell, object? data, bool animate);

    /// <summary>
    /// Pops back to the given page type on the Shell navigation stack, or to the root when it is not found.
    /// </summary>
    /// <typeparam name="TResult">The type of data to return.</typeparam>
    /// <param name="shell">The Shell to navigate.</param>
    /// <param name="pageType">The page type to return to.</param>
    /// <param name="tResult">Data delivered to the target page's ViewModel.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PopToAsync<TResult>(Shell shell, Type pageType, TResult tResult);

    /// <summary>
    /// Pops the Shell navigation stack to its root and returns data to the root ViewModel.
    /// </summary>
    /// <typeparam name="TResult">The type of data to return.</typeparam>
    /// <param name="shell">The Shell to navigate.</param>
    /// <param name="tResult">Data delivered to the root page's ViewModel.</param>
    /// <param name="animate">Whether the transition is animated.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PopToRootAsync<TResult>(Shell shell, TResult tResult, bool animate = true);

    /// <summary>
    /// Pushes several pages in one operation. Only the last page receives the data.
    /// </summary>
    /// <typeparam name="TParameter">The type of data to deliver.</typeparam>
    /// <param name="shell">The Shell to navigate.</param>
    /// <param name="pageTypes">The page types to push, in order.</param>
    /// <param name="tParameter">Data delivered to the final page's ViewModel.</param>
    /// <param name="animate">Whether the final transition is animated.</param>
    /// <param name="animateAllPages">Whether every transition in the stack is animated.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PushMultiStackAsync<TParameter>(Shell shell, List<Type> pageTypes, TParameter tParameter, bool animate, bool animateAllPages);

    /// <summary>
    /// Presents a page modally inside a <see cref="NavigationPage"/> so it can host its own navigation stack.
    /// </summary>
    /// <typeparam name="TParameter">The type of data to deliver.</typeparam>
    /// <param name="shell">The Shell to navigate.</param>
    /// <param name="page">The page shown at the root of the modal stack.</param>
    /// <param name="tParameter">Data delivered to the page's ViewModel.</param>
    /// <param name="animate">Whether the transition is animated.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PushModalWithNavigation<TParameter>(Shell shell, ContentPage page, TParameter? tParameter, bool animate = true);

    /// <summary>
    /// Presents a page modally.
    /// </summary>
    /// <param name="shell">The Shell to navigate.</param>
    /// <param name="pageType">The type of page to present.</param>
    /// <param name="tParameter">Data delivered to the page's ViewModel.</param>
    /// <param name="animate">Whether the transition is animated.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PushModalAsync(Shell shell, Type pageType, object? tParameter, bool animate = true);

    /// <summary>
    /// Delivers data to a page already on the Shell navigation stack without navigating.
    /// </summary>
    /// <param name="shell">The Shell whose stack is searched.</param>
    /// <param name="page">The page type to receive the data.</param>
    /// <param name="data">Data delivered to the matched page's ViewModel.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendDataToPageAsync(Shell shell, Type? page, object? data = null);

    /// <summary>
    /// Shows a popup and delivers data to its ViewModel.
    /// </summary>
    /// <typeparam name="TPopup">The popup type to show.</typeparam>
    /// <param name="shell">The Shell that owns the popup.</param>
    /// <param name="data">Data delivered to the popup's ViewModel.</param>
    /// <param name="onError">Optional handler invoked when the popup fails to show.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ShowPopupAsync<TPopup>(Shell shell, object? data, Action<Exception>? onError = null) where TPopup : Popup;

    /// <summary>
    /// Dismisses a popup and returns data to the current page's ViewModel.
    /// </summary>
    /// <typeparam name="TPopup">The popup type to dismiss.</typeparam>
    /// <param name="shell">The Shell that owns the popup.</param>
    /// <param name="data">Data returned to the current page's ViewModel, or null to return nothing.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DismissPopupAsync<TPopup>(Shell shell, object? data) where TPopup : Popup;
}
