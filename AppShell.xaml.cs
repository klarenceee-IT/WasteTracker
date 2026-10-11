
using Microsoft.Maui.Controls;

namespace WasteTracker;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(
            nameof(SchedulePage),
            typeof(SchedulePage));

        Routing.RegisterRoute(
            nameof(TrackPage),
            typeof(TrackPage));
    }
}
