using CommunityToolkit.Maui.Views;

namespace ShellInject.Interfaces;

public interface IShellInjectNavigation
{
    EventHandler<ShellNavigatedEventArgs>? NavigatedHandler { get; set; }
    object? NavigationParameter { get; set; }
    string RegisterRoute(Type pageType);
    void SetNavigationParameter(object? parameter);
    bool IsReverseNavigation { get; set; }
    Shell? Shell { get; set; }
    void ShellTeardown(Shell shell);
    Task OnShellNavigatedAsync(object? sender, ShellNavigatedEventArgs e);
    Task PushAsync(Shell shell, Type pageType, object? tParameter, bool animate = true);
    Task ReplaceAsync<TParameter>(Shell shell, Type? pageType, TParameter? tParameter, bool animate = true);
    Task ChangeTabAsync<TParameter>(Shell shell, int tabIndex, TParameter? tParameter, bool popToRootFirst);
    Task PopAsync<TResult>(Shell shell, TResult tResult, bool animate = true);
    Task PopModalStackAsync(Shell shell, object? data, bool animate);
    Task PopToAsync<TResult>(Shell shell, Type pageType, TResult tResult);
    Task PopToRootAsync<TResult>(Shell shell, TResult tResult, bool animate = true);
    Task PushMultiStackAsync<TParameter>(Shell shell, List<Type> pageTypes, TParameter tParameter, bool animate, bool animateAllPages);
    Task PushModalWithNavigation<TParameter>(Shell shell, ContentPage page, TParameter? tParameter, bool animate = true);
    Task PushModalAsync(Shell shell, Type pageType, object? tParameter, bool animate = true);
    Task SendDataToPageAsync(Shell shell, Type? page, object? data = null);
    Task ShowPopupAsync<TPopup>(Shell shell, object? data, Action<Exception>? onError = null) where TPopup : Popup;
    Task DismissPopupAsync<TPopup>(Shell shell, object? data) where TPopup : Popup;
}
