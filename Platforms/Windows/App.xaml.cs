using Microsoft.UI.Xaml;
using WasteTracker;

namespace WasteTracker.WinUI;

public partial class App : MauiWinUIApplication
{
    public App()
    {
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}