using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Sample.Services;
using Sample.ContentPages;
using Sample.ViewModels;
using ShellInject;

namespace Sample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        var session = new DemoSession();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseShellInject(options =>
            {
                options.RegisterViewModel<SamplePage2, StackStepViewModel>();
                options.ErrorHandler = ex => session.Record("ShellInject", "Recovered error", ex.Message);
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        
        builder.Services.AddSingleton(session);
        builder.Services.AddSingleton<ISampleService, SampleService>();
        // Register VMs whose lifetime you want to control. Unregistered VMs are still activated by DI.
        builder.Services.AddTransient<DetailsViewModel>();


#if DEBUG
        builder.Logging.AddDebug();
#endif
        
        return builder.Build();
    }
}
