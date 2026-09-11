using ShellInject.Services;

namespace ShellInject;

/// <summary>
/// Base view model that adds strongly typed navigation data handling on top of <see cref="ShellInjectViewModel"/>.
/// Override the typed <c>DataReceivedAsync</c> and <c>ReverseDataReceivedAsync</c> overloads to receive
/// forward and reverse navigation data without casting from <see cref="object"/>.
/// </summary>
/// <typeparam name="TParameter">The expected navigation parameter type.</typeparam>
public abstract class ShellInjectViewModel<TParameter> : ShellInjectViewModel
{
    /// <summary>
    /// Sends typed forward navigation data to the view model asynchronously.
    /// </summary>
    /// <param name="parameter">The data parameter sent when this page was navigated to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task DataReceivedAsync(TParameter? parameter) => Task.CompletedTask;

    /// <summary>
    /// Sends typed reverse navigation data to the view model asynchronously.
    /// </summary>
    /// <param name="parameter">The data parameter returned by a page popped back from this one.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task ReverseDataReceivedAsync(TParameter? parameter) => Task.CompletedTask;

    /// <inheritdoc />
    public override Task DataReceivedAsync(object? parameter)
    {
        if (parameter is TParameter typedParameter)
        {
            return DataReceivedAsync(typedParameter);
        }

        if (parameter is null && default(TParameter) is null)
        {
            return DataReceivedAsync(default(TParameter));
        }

        ShellInjectInitializer.ReportError(BuildMismatchException(parameter));
        return base.DataReceivedAsync(parameter);
    }

    /// <inheritdoc />
    public override Task ReverseDataReceivedAsync(object? parameter)
    {
        if (parameter is TParameter typedParameter)
        {
            return ReverseDataReceivedAsync(typedParameter);
        }

        if (parameter is null && default(TParameter) is null)
        {
            return ReverseDataReceivedAsync(default(TParameter));
        }

        ShellInjectInitializer.ReportError(BuildMismatchException(parameter));
        return base.ReverseDataReceivedAsync(parameter);
    }

    private static InvalidCastException BuildMismatchException(object? parameter)
    {
        var actualType = parameter?.GetType().Name ?? "null";
        return new InvalidCastException($"Navigation data of type {actualType} cannot be delivered to a ViewModel expecting {typeof(TParameter).Name}.");
    }
}
