using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.LifecycleEvents;
using ShellInject.Extensions;
using ShellInject.Interfaces;
using ShellInject.Services;

namespace ShellInject;

/// <summary>
/// Contains extension methods for configuring ShellInject in a Maui app.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "MAUI lifecycle startup glue requires platform integration tests rather than unit tests.")]
public static class ShellInjectMauiBuilderExtensions
{
    private static readonly SemaphoreSlim StartupNavigationSemaphore = new(1, 1);
    private static EventHandler<ShellNavigatedEventArgs>? _navigatedHandler;
    private static PropertyChangedEventHandler? _shellPropertyChangedHandler;
    private static Shell? _trackedShell;
    private static bool _startupNavigationHandled;
    private static bool _pageHandlerMappingInitialized;
    private static bool _windowTrackingInitialized;
    private static readonly object PageHandlerMappingLock = new();

    /// <summary>
    /// Uses ShellInject to configure the ShellInjectMauiBuilderExtensions in a Maui app.
    /// </summary>
    /// <param name="builder">The MauiAppBuilder instance.</param>
    /// <returns>The modified MauiAppBuilder instance.</returns>
    public static MauiAppBuilder UseShellInject(this MauiAppBuilder builder)
    {
        return builder.UseShellInject(null);
    }

    /// <summary>
    /// Uses ShellInject to configure the ShellInjectMauiBuilderExtensions in a Maui app.
    /// </summary>
    /// <param name="builder">The MauiAppBuilder instance.</param>
    /// <param name="configure">Optional configuration for ShellInject behavior.</param>
    /// <returns>The modified MauiAppBuilder instance.</returns>
    public static MauiAppBuilder UseShellInject(this MauiAppBuilder builder, Action<ShellInjectOptions>? configure)
    {
        configure?.Invoke(ShellInjectInitializer.Options);
        builder.Services.AddSingleton<IMauiInitializeService, ShellInjectInitializer>();
        InitializePageHandlerMapping();
        
        builder.ConfigureLifecycleEvents(life =>
        {
#if IOS
            life.AddiOS(i => i.FinishedLaunching((app, launchOptions) =>
            {
                InitializeWindowTracking();
                return true;
            }));
#endif
#if ANDROID 
            life.AddAndroid(a => a.OnCreate((activity, state) =>
            {
                InitializeWindowTracking();
            }));
#endif
        });

        
        return builder;
    }

    private static void InitializePageHandlerMapping()
    {
        lock (PageHandlerMappingLock)
        {
            if (_pageHandlerMappingInitialized)
            {
                return;
            }

            _pageHandlerMappingInitialized = true;
            PageHandler.Mapper.AppendToMapping("ShellInject.ViewModelConvention", (_, view) =>
            {
                if (view is BindableObject bindable)
                {
                    ShellInjectPageExtensions.TryBindViewModelByConvention(bindable);
                }
            });
        }
    }
    
    internal static void InitializeWindowTracking()
    {
        if (_windowTrackingInitialized)
        {
            return;
        }

        if (Application.Current is not { } app)
        {
            return;
        }

        _windowTrackingInitialized = true;
        app.PropertyChanged += OnApplicationPropertyChanged;
        if (app.Windows is INotifyCollectionChanged windowNotifier)
        {
            windowNotifier.CollectionChanged += OnWindowsCollectionChanged;
        }

        TryAttachShell(GetRootPage(app));
    }

    private static void OnApplicationPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(Application.Windows))
        {
            return;
        }

        if (Application.Current is not { } app)
        {
            return;
        }

        TryAttachShell(GetRootPage(app));
    }

    private static void OnWindowsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (Application.Current is not { } app)
        {
            return;
        }

        TryAttachShell(GetRootPage(app));
    }

    private static Page? GetRootPage(Application app)
    {
        if (app.Windows.Count == 0)
        {
            return null;
        }

        return app.Windows[0].Page;
    }

    private static void TryAttachShell(Page? rootPage)
    {
        if (rootPage is Shell shell)
        {
            AttachShell(shell);
            return;
        }

        DetachShell();
    }

    private static void AttachShell(Shell shell)
    {
        if (ReferenceEquals(_trackedShell, shell))
        {
            if (!_startupNavigationHandled && _navigatedHandler is null)
            {
                AttachShellNavigatedHandler(shell);
                _ = TryHandleInitialNavigationAsync(shell);
            }

            if (_shellPropertyChangedHandler is null)
            {
                AttachShellPropertyChangedHandler(shell);
            }

            TryBindCurrentPage(shell);

            return;
        }

        DetachShell();
        _trackedShell = shell;
        _startupNavigationHandled = false;
        AttachShellNavigatedHandler(shell);
        AttachShellPropertyChangedHandler(shell);
        TryBindCurrentPage(shell);
        _ = TryHandleInitialNavigationAsync(shell);
    }

    private static void AttachShellNavigatedHandler(Shell shell)
    {
        _navigatedHandler = async void (s, e) =>
        {
            if (s is not Shell navigatedShell)
            {
                return;
            }

            await TryHandleInitialNavigationAsync(navigatedShell);
        };

        shell.Navigated += _navigatedHandler;
    }

    private static void AttachShellPropertyChangedHandler(Shell shell)
    {
        _shellPropertyChangedHandler = (s, e) =>
        {
            if (s is not Shell changedShell)
            {
                return;
            }

            if (e.PropertyName is nameof(Shell.CurrentPage) or nameof(Shell.CurrentItem))
            {
                TryBindCurrentPage(changedShell);
            }
        };

        shell.PropertyChanged += _shellPropertyChangedHandler;
    }

    private static void DetachShell()
    {
        if (_trackedShell is not null && _navigatedHandler is not null)
        {
            _trackedShell.Navigated -= _navigatedHandler;
        }

        if (_trackedShell is not null && _shellPropertyChangedHandler is not null)
        {
            _trackedShell.PropertyChanged -= _shellPropertyChangedHandler;
        }

        _trackedShell = null;
        _navigatedHandler = null;
        _shellPropertyChangedHandler = null;
        _startupNavigationHandled = false;
    }

    private static void TryBindCurrentPage(Shell shell)
    {
        if (shell.CurrentPage is not null)
        {
            ShellInjectPageExtensions.TryBindViewModelByConvention(shell.CurrentPage);
        }
    }

    private static void DetachShellNavigatedHandler(Shell shell)
    {
        if (_navigatedHandler is null)
        {
            return;
        }

        shell.Navigated -= _navigatedHandler;
        _navigatedHandler = null;
    }

    private static async Task TryHandleInitialNavigationAsync(Shell shell)
    {
        if (_startupNavigationHandled || !ReferenceEquals(_trackedShell, shell))
        {
            return;
        }

        await StartupNavigationSemaphore.WaitAsync();
        try
        {
            if (_startupNavigationHandled || !ReferenceEquals(_trackedShell, shell))
            {
                return;
            }

            if (await OnShellNavigatedAsync(shell))
            {
                _startupNavigationHandled = true;
                DetachShellNavigatedHandler(shell);
            }
        }
        catch
        {
            // just catch it
        }
        finally
        {
            StartupNavigationSemaphore.Release();
        }
    }

    private static async Task<bool> OnShellNavigatedAsync(Shell shell)
    {
        var firstPage = shell.CurrentPage;
        if (firstPage is null)
        {
            return false;
        }

        var boundByConvention = ShellInjectPageExtensions.TryBindViewModelByConvention(firstPage);

        if (firstPage.BindingContext is not IShellInjectShellViewModel vm)
        {
            return false;
        }

        if (boundByConvention)
        {
            vm.OnAppearing();
        }

        await vm.OnAppearedAsync();

        if (!vm.IsInitialized)
        {
            await vm.InitializedAsync();
            vm.IsInitialized = true;
        }

        return true;
    }
}
