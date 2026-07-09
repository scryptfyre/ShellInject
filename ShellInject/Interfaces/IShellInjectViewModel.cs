namespace ShellInject.Interfaces;

/// <summary>
/// Represents the interface for the IShellInjectShellViewModel.
/// </summary>
public interface IShellInjectShellViewModel
{
    bool IsInitialized { get; set; }

    /// <summary>
    /// Executes initialization logic for the view model asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous initialization operation.</returns>
    Task InitializedAsync();

    /// <summary>
    /// Sends data to the view model asynchronously.
    /// </summary>
    /// <param name="parameter">The data parameter to be sent to the view model.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DataReceivedAsync(object? parameter);

    /// <summary>
    /// Reverses the data received asynchronously.
    /// </summary>
    /// <param name="parameter">The parameter of type object that contains the data to be reversed.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method reverses the data received asynchronously. The parameter argument should contain the data to be reversed.
    /// </remarks>
    Task ReverseDataReceivedAsync(object? parameter);

    /// <summary>
    /// Executes operations asynchronously when the page appears.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task OnAppearedAsync();

    // /// <summary>
    // /// OnAppearing Method
    // /// </summary>
    void OnAppearing();

    /// <summary>
    /// Executes tasks or operations when the view is about to disappear from the screen.
    /// </summary>
    void OnDisAppearing();
}