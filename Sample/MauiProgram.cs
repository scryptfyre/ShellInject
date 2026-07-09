using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Sample.Services;
using ShellInject;

namespace Sample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseShellInject()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        
        builder.Services.AddScoped<ISampleService, SampleService>();


#if DEBUG
        builder.Logging.AddDebug();
#endif
        
        return builder.Build();
    }
}