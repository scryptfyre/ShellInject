namespace ShellInject;

/// <summary>
/// Configures ShellInject startup behavior.
/// </summary>
public sealed class ShellInjectOptions
{
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
}
