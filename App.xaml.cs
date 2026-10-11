
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace WasteTracker;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Clear the previous placeholder login state.
        Preferences.Remove("AuthToken");
        Preferences.Remove("LoggedInUsername");
        Preferences.Remove("Username");

        // Start with a clean user session.
        AppState.Data = new UserData();
    }

    protected override Window CreateWindow(
        IActivationState? activationState)
    {
        return new Window(new WelcomePage());
    }
}
