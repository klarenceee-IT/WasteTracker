
using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Net.Http.Json;
using System.Text.Json;

namespace WasteTracker;

public partial class HomePage : ContentPage
{
    private static readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("https://wastetracker-api.onrender.com")
    };
    public HomePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Dispatcher.Dispatch(async () =>
        {
            await LoadHomePageDataAsync();
        });
    }

    private async Task LoadHomePageDataAsync()
    {
        try
        {
            // 1. Populate Waste Categories on the UI
            if (Cats != null)
            {
                BindableLayout.SetItemsSource(Cats, AppState.Types);
            }

            string savedUsername = Preferences.Get("LoggedInUsername", Preferences.Get("Username", "User"));

            if (WelcomeUserLabel != null)
            {
                WelcomeUserLabel.Text = savedUsername;
            }

            if (FindByName("Hello") is Label helloLabel)
            {
                helloLabel.Text = $"Hello, {savedUsername.Split(' ')[0]}";
            }

            // 2. Fetch history from Render backend
            HttpResponseMessage response = await _httpClient.GetAsync($"/api/waste/history/{savedUsername}");

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var logs = JsonSerializer.Deserialize<List<WasteLog>>(json, options);

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
            System.Diagnostics.Debug.WriteLine($"Error loading home data: {ex.Message}");
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

    private async void Cat_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject bindable && bindable.BindingContext is WasteType wasteType)
        {
            if (await AppState.PromptLog(this, wasteType))
            {
                // Refresh home page totals immediately after logging
                await LoadHomePageDataAsync();
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

}
