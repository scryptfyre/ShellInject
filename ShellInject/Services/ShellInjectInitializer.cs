namespace ShellInject.Services;

/// <summary>
/// The ShellInjectInitializer class is responsible for initializing the dependency injection container
/// and providing access to registered services in a .NET MAUI application. This class implements
/// the IMauiInitializeService interface to be invoked during the app's initialization phase.
/// </summary>
internal class ShellInjectInitializer : IMauiInitializeService
{
    private const string ServiceProviderNotInitializedMessage =
        "ShellInject has not been constructed or is not properly configured.";

    /// <summary>
    /// A static variable holding the root <see cref="IServiceProvider"/> instance
    /// used for resolving dependencies in the ShellInject dependency injection system.
    /// This variable is initialized during the application startup phase.
    /// </summary>
    internal static IServiceProvider? ServiceProvider;

    /// <summary>
    /// Gets the configured ShellInject options.
    /// </summary>
    internal static ShellInjectOptions Options { get; } = new();

    /// <summary>
    /// Retrieves a service of the specified type from the DI container.
    /// Throws an <see cref="InvalidOperationException"/> if the service provider has not been initialized
    /// or if the requested service of type <typeparamref name="T"/> cannot be resolved.
    /// </summary>
    /// <typeparam name="T">The type of service to resolve.</typeparam>
    /// <returns>The requested service instance of type <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the service provider is null, indicating that the dependency injection system
    /// has not been properly initialized or configured.
    /// </exception>
    internal static T GetRequiredService<T>() where T : notnull
    {
        var provider = GetServiceProvider();
        var service = provider.GetService<T>();
        if (service is null)
        {
            throw new InvalidOperationException($"No service for type {typeof(T).FullName} has been registered.");
        }

        return service;
    }

    /// <summary>
    /// Retrieves a registered service of the specified type from the dependency injection system.
    /// Returns null if the specified service is not registered.
    /// </summary>
    /// <typeparam name="T">The type of the service to resolve.</typeparam>
    /// <returns>The resolved service instance of type <typeparamref name="T"/> or null if not registered.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the service provider has not been initialized.
    /// </exception>
    internal static T? GetService<T>()
    {
        var provider = GetServiceProvider();
        return provider.GetService<T>();
    }

    private static IServiceProvider GetServiceProvider()
    {
        return ServiceProvider ?? throw new InvalidOperationException(ServiceProviderNotInitializedMessage);
    }

    /// <summary>
    /// Reports a recovered internal error to the configured <see cref="ShellInjectOptions.ErrorHandler"/>, if any.
    /// Exceptions thrown by the handler are ignored so diagnostics never break navigation behavior.
    /// </summary>
    internal static void ReportError(Exception exception)
    {
        try
        {
            Options.ErrorHandler?.Invoke(exception);
        }
        catch (Exception handlerException)
        {
            _ = handlerException;
        }
    }

    // NOTE:  -Called automatically by MAUI after the final service provider is built:
    /// <summary>
    /// Initializes the dependency injection system for the application by assigning the provided service provider
    /// to the static <see cref="ServiceProvider"/> property.
    /// </summary>
    /// <param name="services">The <see cref="IServiceProvider"/> instance that contains the registered services.</param>
    public void Initialize(IServiceProvider services)
    {
        ServiceProvider = services; 
        ShellInjectMauiBuilderExtensions.InitializeWindowTracking();
    }
}
