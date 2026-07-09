using CommunityToolkit.Maui.Views;
using ShellInject.Interfaces;
using ShellInject.Services;

namespace ShellInject.Extensions;

/// <summary>
/// Provides attached properties for extending the functionality of Shell pages in a Maui application.
/// </summary>
public static class ShellInjectPageExtensions
{
    // A private helper class to store event handlers so they can be detached later.
    private class PageLifecycleToken
    {
        public EventHandler AppearingHandler { get; }
        public EventHandler DisappearingHandler { get; }

        public PageLifecycleToken(EventHandler appearing, EventHandler disappearing)
        {
            AppearingHandler = appearing;
            DisappearingHandler = disappearing;
        }
    }

    // An attached property to hold the lifecycle token object on each page.
    private static readonly BindableProperty PageLifecycleTokenProperty =
        BindableProperty.CreateAttached(
            "PageLifecycleToken",
            typeof(PageLifecycleToken),
            typeof(ShellInjectPageExtensions),
            null);

    private static void DetachPageLifecycleHandlers(ContentPage page)
    {
        if (page.GetValue(PageLifecycleTokenProperty) is not PageLifecycleToken token)
        {
            return;
        }

        page.Appearing -= token.AppearingHandler;
        page.Disappearing -= token.DisappearingHandler;
        page.SetValue(PageLifecycleTokenProperty, null);
    }

    
    // Attached Properties for ViewModel Type

    /// <summary>
    /// Provides attached properties for extending the functionality of Shell pages in a Maui application.
    /// </summary>
    public static readonly BindableProperty ViewModelTypeProperty =
        BindableProperty.CreateAttached(
            "ViewModelType",
            typeof(Type),
            typeof(ShellInjectPageExtensions),
            null,
            propertyChanged: OnViewModelTypePropertyChanged);

    /// <summary>
    /// Called when the value of the ViewModelType attached property changes.
    /// </summary>
    /// <param name="bindable">The bindable object on which the property is attached.</param>
    /// <param name="oldValue">The old value of the property.</param>
    /// <param name="newValue">The new value of the property.</param>
    private static void OnViewModelTypePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (newValue is not Type viewModelType)
        {
            return;
        }
         
        try
        {
            BindViewModel(bindable, viewModelType, replaceExistingBindingContext: true);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    internal static bool TryBindViewModelByConvention(BindableObject bindable)
    {
        ArgumentNullException.ThrowIfNull(bindable);

        if (!ShellInjectInitializer.Options.AutoBindViewModelsByConvention || GetBindingContext(bindable) is not null)
        {
            return false;
        }

        if (bindable.GetValue(ViewModelTypeProperty) is Type)
        {
            return false;
        }

        var viewModelType = ResolveViewModelTypeByConvention(bindable.GetType());
        if (viewModelType is null)
        {
            return false;
        }

        try
        {
            BindViewModel(bindable, viewModelType, replaceExistingBindingContext: false);
            return GetBindingContext(bindable) is not null;
        }
        catch
        {
            return false;
        }
    }

    private static Type? ResolveViewModelTypeByConvention(Type viewType)
    {
        var candidateNames = BuildViewModelCandidateNames(viewType).ToArray();
        if (candidateNames.Length == 0)
        {
            return null;
        }

        foreach (var candidateName in candidateNames)
        {
            var preferredType = viewType.Assembly.GetType(BuildPreferredFullName(viewType, candidateName), throwOnError: false);
            if (preferredType is not null)
            {
                return preferredType;
            }

            var matches = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.IsDynamic)
                .SelectMany(GetLoadableTypes)
                .Where(type => type.Name == candidateName)
                .Distinct()
                .Take(2)
                .ToArray();

            if (matches.Length == 1)
            {
                return matches[0];
            }

            if (matches.Length > 1)
            {
                return null;
            }
        }

        return null;
    }

    private static IEnumerable<string> BuildViewModelCandidateNames(Type viewType)
    {
        var options = ShellInjectInitializer.Options;
        var viewName = viewType.Name;
        var names = new List<string>();

        if (!string.IsNullOrWhiteSpace(options.PageSuffix) && viewName.EndsWith(options.PageSuffix, StringComparison.Ordinal))
        {
            names.Add($"{viewName[..^options.PageSuffix.Length]}{options.ViewModelSuffix}");
        }

        names.Add($"{viewName}{options.ViewModelSuffix}");
        return names.Distinct(StringComparer.Ordinal);
    }

    private static string BuildPreferredFullName(Type viewType, string candidateName)
    {
        var viewNamespace = viewType.Namespace;
        if (string.IsNullOrWhiteSpace(viewNamespace))
        {
            return candidateName;
        }

        var preferredNamespace = viewNamespace
            .Replace(".ContentPages", ".ViewModels", StringComparison.Ordinal)
            .Replace(".Pages", ".ViewModels", StringComparison.Ordinal)
            .Replace(".Views", ".ViewModels", StringComparison.Ordinal);

        return $"{preferredNamespace}.{candidateName}";
    }

    private static IEnumerable<Type> GetLoadableTypes(System.Reflection.Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (System.Reflection.ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(type => type is not null).Cast<Type>();
        }
    }

    private static void BindViewModel(BindableObject bindable, Type viewModelType, bool replaceExistingBindingContext)
    {
        if (!replaceExistingBindingContext && GetBindingContext(bindable) is not null)
        {
            return;
        }

        var viewModelInstance = ResolveViewModel(viewModelType);

        if (bindable is ContentPage page)
        {
            DetachPageLifecycleHandlers(page);
        }

        SetBindingContext(bindable, viewModelInstance);

        if (bindable is ContentPage contentPage && viewModelInstance is IShellInjectShellViewModel vmInstance)
        {
            AttachPageLifecycleHandlers(contentPage, vmInstance);
        }
    }

    private static object? GetBindingContext(BindableObject bindable)
    {
        return bindable switch
        {
            Popup popup => popup.BindingContext,
            VisualElement visualElement => visualElement.BindingContext,
            _ => null
        };
    }

    private static void SetBindingContext(BindableObject bindable, object viewModelInstance)
    {
        switch (bindable)
        {
            case Popup popup:
                popup.BindingContext = viewModelInstance;
                break;
            case VisualElement visualElement:
                visualElement.BindingContext = viewModelInstance;
                break;
        }
    }

    private static void AttachPageLifecycleHandlers(ContentPage bindablePage, IShellInjectShellViewModel vmInstance)
    {
        EventHandler appearingHandler = (s, e) =>
        {
            try
            {
                vmInstance.OnAppearing();
            }
            catch
            {
                // ignored
            }
        };

        EventHandler disappearingHandler = (s, e) =>
        {
            try
            {
                vmInstance.OnDisAppearing();
            }
            catch
            {
                // ignored
            }
        };

        bindablePage.Appearing += appearingHandler;
        bindablePage.Disappearing += disappearingHandler;
        bindablePage.SetValue(PageLifecycleTokenProperty, new PageLifecycleToken(appearingHandler, disappearingHandler));
    }

    /// <summary>
    /// Resolves and creates an instance of the specified ViewModel type.
    /// </summary>
    /// <param name="viewModelType">The type of the ViewModel to be resolved.</param>
    /// <returns>
    /// An object instance of the specified ViewModel type. May return an existing instance
    /// if registered in the dependency injection container or create a new one.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the ViewModel instance cannot be created either because the type is invalid
    /// or the ServiceProvider is not properly configured.
    /// </exception>
    private static object ResolveViewModel(Type viewModelType)
    {
        if (Injector.ServiceProvider is not { } provider)
        {
            // If there's no service provider, just do a plain Activator create:
            return Activator.CreateInstance(viewModelType)
                   ?? throw new InvalidOperationException(
                       $"Unable to create instance of ViewModel. Type: {viewModelType.FullName}.");
        }
        
        return ActivatorUtilities.GetServiceOrCreateInstance(provider, viewModelType)
               ?? throw new InvalidOperationException($"Unable to create instance of ViewModel. Type: {viewModelType.FullName}.");

    }

    /// <summary>
    /// Retrieves the value of the ViewModelType attached property from the specified bindable object.
    /// </summary>
    /// <param name="obj">The bindable object from which to retrieve the ViewModelType.</param>
    /// <returns>The type of the ViewModel set on the bindable object.</returns>
    public static Type GetViewModelType(BindableObject obj)
    {
        return (Type)obj.GetValue(ViewModelTypeProperty);
    }

    /// <summary>
    /// Sets the value of the ViewModelType attached property for the specified bindable object.
    /// </summary>
    /// <param name="obj">The bindable object on which the ViewModelType property is being set.</param>
    /// <param name="value">The Type value to be set for the ViewModelType property.</param>
    public static void SetViewModelType(BindableObject obj, Type value)
    {
        obj.SetValue(ViewModelTypeProperty, value);
    }
}
