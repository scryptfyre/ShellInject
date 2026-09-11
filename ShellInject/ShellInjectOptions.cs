using System.Diagnostics.CodeAnalysis;

namespace ShellInject;

/// <summary>
/// Configures ShellInject startup behavior.
/// </summary>
public sealed class ShellInjectOptions
{
    private readonly Dictionary<Type, Type> _explicitViewModelMap = new();
    private readonly object _explicitViewModelLock = new();

    /// <summary>
    /// Gets or sets a value indicating whether ShellInject should bind pages and popups to ViewModels by naming convention.
    /// </summary>
    public bool AutoBindViewModelsByConvention { get; set; } = true;

    /// <summary>
    /// Gets or sets the page suffix removed before appending <see cref="ViewModelSuffix"/>.
    /// </summary>
    public string PageSuffix { get; set; } = "Page";

    /// <summary>
    /// Gets or sets the ViewModel suffix used for convention binding.
    /// </summary>
    public string ViewModelSuffix { get; set; } = "ViewModel";

    /// <summary>
    /// Gets or sets an optional handler invoked when ShellInject recovers from an internal error,
    /// such as a failed convention lookup fallback, a ViewModel creation failure, or a swallowed
    /// navigation lifecycle exception. Exceptions thrown by the handler itself are ignored.
    /// </summary>
    public Action<Exception>? ErrorHandler { get; set; }

    /// <summary>
    /// Registers an explicit view-to-ViewModel mapping that bypasses convention-based type discovery.
    /// Prefer this API in trimming or NativeAOT builds so ViewModel types do not need to be discovered by name.
    /// </summary>
    /// <typeparam name="TView">The view type (page or popup) to bind.</typeparam>
    /// <typeparam name="TViewModel">The ViewModel type to construct for the view.</typeparam>
    /// <returns>The current <see cref="ShellInjectOptions"/> instance for chaining.</returns>
    public ShellInjectOptions RegisterViewModel<TView, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>()
        where TViewModel : class
    {
        return RegisterViewModel(typeof(TView), typeof(TViewModel));
    }

    /// <summary>
    /// Registers an explicit view-to-ViewModel mapping that bypasses convention-based type discovery.
    /// Prefer this API in trimming or NativeAOT builds so ViewModel types do not need to be discovered by name.
    /// </summary>
    /// <param name="viewType">The view type (page or popup) to bind.</param>
    /// <param name="viewModelType">The ViewModel type to construct for the view.</param>
    /// <returns>The current <see cref="ShellInjectOptions"/> instance for chaining.</returns>
    public ShellInjectOptions RegisterViewModel(
        Type viewType,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type viewModelType)
    {
        ArgumentNullException.ThrowIfNull(viewType);
        ArgumentNullException.ThrowIfNull(viewModelType);

        lock (_explicitViewModelLock)
        {
            _explicitViewModelMap[viewType] = viewModelType;
        }

        return this;
    }

    internal bool TryGetRegisteredViewModel(Type viewType, [NotNullWhen(true)] out Type? viewModelType)
    {
        lock (_explicitViewModelLock)
        {
            return _explicitViewModelMap.TryGetValue(viewType, out viewModelType);
        }
    }
}
