using Microsoft.Extensions.Logging;
using System.Globalization;

namespace ParkSystemApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SawarabiGothic-Regular.ttf", "SawarabiGothic");
                    fonts.AddFont("FontAwesome.ttf", "FontAwesome");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                });

                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("it-IT");
                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("it-IT");

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
