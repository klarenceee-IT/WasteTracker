
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace WasteTracker;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        LoadUserProfile();
    }

    private void LoadUserProfile()
    {
        string username = AppState.Data.Name;

        if (string.IsNullOrWhiteSpace(username))
        {
            username = Preferences.Get(
                "LoggedInUsername",
                "User");
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            username = "User";
        }

        // Username displayed at the top.
        ProfileUsernameLabel.Text = username;

        // Username displayed in the profile card.
        NameL.Text = username;

        // Display the first letter as the avatar.
        Initials.Text = username.Length > 0
            ? username[..1].ToUpperInvariant()
            : "U";

        // No email or points are supplied by the
        // current authentication API.
        EmailL.Text = "Email not provided";
        PointsL.Text = "—";
    }

    private async void Row_Tapped(
        object? sender,
        EventArgs e)
    {
        if (sender is Element element &&
            element.BindingContext is MenuItem selectedItem)
        {
            await DisplayAlertAsync(
                selectedItem.Title,
                selectedItem.Sub,
                "OK");
        }
    }

    private async void Logout_Clicked(
        object? sender,
        EventArgs e)
    {
        bool confirm = await DisplayAlertAsync(
            "Log Out",
            "Are you sure you want to log out?",
            "Yes",
            "Cancel");

        if (!confirm)
            return;

        // Clear local login information.
        Preferences.Remove("AuthToken");
        Preferences.Remove("LoggedInUsername");
        Preferences.Remove("Username");

        // Clear previous user's in-memory information.
        AppState.Data = new UserData();

        // Return to login.
        var currentWindow = Window;

        if (currentWindow != null)
        {
            currentWindow.Page = new WelcomePage();
        }
    }
}

// Keep this class only once in your project.
public class MenuItem
{
    public string Icon { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Sub { get; set; } = string.Empty;
}
