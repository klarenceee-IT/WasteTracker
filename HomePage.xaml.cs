
using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Net.Http.Json;
using System.Text.Json;

namespace WasteTracker;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();

        System.Diagnostics.Debug.WriteLine(
            "HOME TEST: XAML loaded successfully.");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        System.Diagnostics.Debug.WriteLine(
            "HOME TEST: OnAppearing started.");

        LoadHomeData();
    }

    private void LoadHomeData()
    {
        try
        {
            string username = Preferences.Get(
                "LoggedInUsername",
                "User");

            if (string.IsNullOrWhiteSpace(username))
            {
                username = "User";
            }

            WelcomeUserLabel.Text =
                $"Welcome, {username}!";

            System.Diagnostics.Debug.WriteLine(
                "HOME TEST: Username loaded.");

            var userData = AppState.Data;

            DateTime today = DateTime.Today;

            double totalKg = userData.History
                .Where(x =>
                    x.Date.Year == today.Year &&
                    x.Date.Month == today.Month)
                .Sum(x => x.Kg);

            Total.Text = $"{totalKg:0.#} kg";

            System.Diagnostics.Debug.WriteLine(
                "HOME TEST: Waste total loaded.");

            var pickup = userData.Next;

            NextL.Text = pickup == null
                ? "Nothing scheduled yet"
                : $"{pickup.Date?.ToString("ddd, d MMM") ?? "No date"} · {pickup.Slot}";

            System.Diagnostics.Debug.WriteLine(
                "HOME TEST: Loading category cards.");

            BindableLayout.SetItemsSource(
                Cats,
                AppState.Types);

            System.Diagnostics.Debug.WriteLine(
                "HOME TEST: Category cards loaded.");

        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"HOME ERROR: {ex}");

            WelcomeUserLabel.Text =
                "Welcome to WasteTracker!";
        }
    }

    private async void Quick_Clicked(
        object? sender,
        EventArgs e)
    {
        if (sender is not Button button)
            return;

        try
        {
            switch (button.ClassId)
            {
                case "schedule":
                    await Shell.Current.GoToAsync(
                        nameof(SchedulePage));
                    break;

                case "pickup":
                    await Shell.Current.GoToAsync(
                        nameof(TrackPage));
                    break;

                case "dropoff":
                    await Shell.Current.GoToAsync(
                        "//MapPage");
                    break;

                case "tutorial":
                    await DisplayAlertAsync(
                        "Tutorial",
                        "1. Sort your waste.\n" +
                        "2. Select a category.\n" +
                        "3. Schedule a pickup.\n" +
                        "4. Track your collection.",
                        "OK");
                    break;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Navigation Error",
                ex.Message,
                "OK");
        }
    }

        private async void SeeAll_Tapped(
        object? sender,
        TappedEventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("//CategoriesPage");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Navigation Error",
                ex.Message,
                "OK");
        }
    }

    private async void Next_Tapped(
        object? sender,
        TappedEventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync(
                nameof(TrackPage));
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Navigation Error",
                ex.Message,
                "OK");
        }
    }

    private async void Cat_Tapped(
        object? sender,
        TappedEventArgs e)
    {
        if (sender is BindableObject element &&
            element.BindingContext is WasteType wasteType)
        {
            try
            {
                bool saved = await AppState.PromptLog(
                    this,
                    wasteType);

                if (saved)
                {
                    LoadHomeData();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync(
                    "Error",
                    ex.Message,
                    "OK");
            }
        }
    }
    
    private async void Schedule_Tapped(
        object? sender,
        TappedEventArgs e)
    {
        await OpenDetailPage(nameof(SchedulePage));
    }

    private async void Track_Tapped(
        object? sender,
        TappedEventArgs e)
    {
        await OpenDetailPage(nameof(TrackPage));
    }

    
    private async void Map_Tapped(
        object? sender,
        TappedEventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("//MapPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Map navigation error: {ex}");

            await DisplayAlertAsync(
                "Navigation Error",
                ex.Message,
                "OK");
        }
    }


    private async void Guide_Tapped(
        object? sender,
        TappedEventArgs e)
    {
        await DisplayAlertAsync(
            "Waste Guide",
            "Separate recyclable, organic, hazardous, and general waste into the correct categories.",
            "OK");
    }

    private async Task OpenDetailPage(string route)
    {
        try
        {
            await Shell.Current.GoToAsync(route);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Navigation error: {ex}");

            await DisplayAlertAsync(
                "Navigation Error",
                ex.Message,
                "OK");
        }
    }
    private async Task LoadHomePageDataAsync()
{
    try
    {
        string savedUsername = Preferences.Get("LoggedInUsername", Preferences.Get("Username", "User"));

        if (WelcomeUserLabel != null)
        {
            WelcomeUserLabel.Text = savedUsername;
        }

        // Fetch user history from Render backend API
        using var client = new HttpClient();
        var response = await client.GetAsync($"https://wastetracker-api.onrender.com/api/waste/history/{savedUsername}");

        if (response.IsSuccessStatusCode)
        {
            var logs = await response.Content.ReadFromJsonAsync<List<WasteLog>>();
            if (logs != null)
            {
                AppState.Data.History = logs;

                var now = DateTime.Today;
                double monthlyTotal = logs
                    .Where(e => e.Date.Year == now.Year && e.Date.Month == now.Month)
                    .Sum(e => e.Kg);

                Total.Text = $"{monthlyTotal:0.#} kg";
            }
        }
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error fetching user history: {ex.Message}");
    }
}

}
