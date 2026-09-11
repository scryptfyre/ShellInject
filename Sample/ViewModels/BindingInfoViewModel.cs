using Sample.Services;

namespace Sample.ViewModels;

public sealed class BindingInfoViewModel(ISampleService service)
{
    public string Message { get; } = service.GetMessage();
}
