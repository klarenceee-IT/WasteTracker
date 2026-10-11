using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;

namespace WasteTracker;

public partial class MapPage : ContentPage
{
    private bool _locationRequested;

    public MapPage()
    {
        InitializeComponent();

        EcoMap.MapClicked += OnMapClicked;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_locationRequested)
        {
            _locationRequested = true;

            await CenterMapOnCurrentLocationAsync();
        }
    }

    private async Task CenterMapOnCurrentLocationAsync()
    {
        try
        {
            PermissionStatus permission =
                await Permissions.RequestAsync<
                    Permissions.LocationWhenInUse>();

            if (permission != PermissionStatus.Granted)
            {
                MoveToDefaultLocation();

                await DisplayAlert(
                    "Location Permission",
                    "Location permission was not granted. The map was opened at the default location.",
                    "OK");

                return;
            }

            Location? location =
                await Geolocation.GetLastKnownLocationAsync();

            if (location == null)
            {
                location =
                    await Geolocation.GetLocationAsync(
                        new GeolocationRequest(
                            GeolocationAccuracy.Medium,
                            TimeSpan.FromSeconds(10)));
            }

            if (location != null)
            {
                MapSpan span =
                    MapSpan.FromCenterAndRadius(
                        location,
                        Distance.FromKilometers(2));

                EcoMap.MoveToRegion(span);
            }
            else
            {
                MoveToDefaultLocation();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Map location error: {ex}");

            MoveToDefaultLocation();
        }
    }

    private void MoveToDefaultLocation()
    {
        // Preserves the default location from your original app.
        var location =
            new Location(
                9.7392,
                118.7353);

        var span =
            MapSpan.FromCenterAndRadius(
                location,
                Distance.FromKilometers(5));

        EcoMap.MoveToRegion(span);
    }

    private async void OnCenterLocationClicked(
        object? sender,
        EventArgs e)
    {
        await CenterMapOnCurrentLocationAsync();
    }

    private async void OnBackClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private void OnMapClicked(
        object? sender,
        MapClickedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine(
            $"Map clicked: {e.Location.Latitude}, {e.Location.Longitude}");
    }

    private async void OnPinMarkerClicked(
    object? sender,
    PinClickedEventArgs e)
    {
        if (sender is not Pin pin)
            return;

        e.HideInfoWindow = false;

        bool navigate = await DisplayAlertAsync(
            pin.Label,
            pin.Address ?? "Open directions to this location?",
            "Directions",
            "Cancel");

        if (!navigate)
            return;

        try
        {
            await Microsoft.Maui.ApplicationModel.Map.Default.OpenAsync(
                pin.Location,
                new Microsoft.Maui.ApplicationModel.MapLaunchOptions
                {
                    Name = pin.Label,
                    NavigationMode =
                        Microsoft.Maui.ApplicationModel.NavigationMode.Driving
                });
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Map Error",
                ex.Message,
                "OK");
        }
    }

    public void AddDropOffPoint(
    string name,
    string address,
    double latitude,
    double longitude)
{
    var pin = new Pin
    {
        Label = name,
        Address = address,
        Location = new Location(latitude, longitude)
    };

    pin.MarkerClicked += OnPinMarkerClicked;

    EcoMap.Pins.Add(pin);
}
}