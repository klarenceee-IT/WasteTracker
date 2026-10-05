using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
#if MAUI_DEVFLOW
using Microsoft.Maui.DevFlow.Agent;
#endif

using WasteTracker;

namespace WasteTracker; // Or ensure namespace is set to WasteTracker

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
#if MAUI_DEVFLOW
            .AddMauiDevFlowAgent()
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        return builder.Build();
    }
}