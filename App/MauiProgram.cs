using App.ViewModel;
using Microcharts.Maui;
using Microsoft.Extensions.Logging;

namespace App;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMicrocharts()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
		builder.Logging.AddDebug();
#endif
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<MainViewModel>();

        builder.Services.AddTransient<SecondPage>();
        builder.Services.AddTransient<SecondViewModel>();

        builder.Services.AddTransient<StatPage>();
        builder.Services.AddTransient<StatViewModel>();
        return builder.Build();
    }
}
