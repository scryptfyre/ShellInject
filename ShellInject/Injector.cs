using ShellInject.Services;

namespace ShellInject;

/// <summary>
/// Provides static methods for resolving services within the application via dependency injection.
/// This class acts as a wrapper around the available service provider and simplifies service resolution.
/// </summary>
public static class Injector
{
    internal static IServiceProvider? ServiceProvider => ShellInjectInitializer.ServiceProvider;

    /// <summary>
    /// Resolves and returns a required service of the specified type from the dependency injection container.
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve.</typeparam>
    /// <returns>The resolved service instance of type <typeparamref name="TService"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the service provider is not initialized or if the service is not registered.</exception>
    public static TService GetRequiredService<TService>() where TService : notnull
    {
        return ShellInjectInitializer.GetRequiredService<TService>();
    }

    /// <summary>
    /// Resolves and returns a service of the specified type from the dependency injection container.
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve.</typeparam>
    /// <returns>The resolved service instance of type <typeparamref name="TService"/> or null if not registered.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the service provider is not initialized.</exception>
    public static TService? GetService<TService>()
    {
        return ShellInjectInitializer.GetService<TService>();
    }
}
