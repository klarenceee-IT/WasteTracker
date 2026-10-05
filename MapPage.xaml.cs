using Microsoft.Maui.ApplicationModel;
namespace WasteTracker;

public partial class MapPage : ContentPage
{
    readonly MapDrawable map = new();
    List<DropSpot> shown = new();
    string filter = "All";
    int sel;

    public MapPage() { InitializeComponent(); MapView.Drawable = map; Apply(); }

    void Apply()
    {
        Ui.Chips(Chips, filter);
        var text = Search.Text ?? "";
        shown = AppState.Spots.Where(s => (filter == "All" || s.Type == filter) && s.Name.Contains(text, StringComparison.OrdinalIgnoreCase)).ToList();
        map.Pins = shown.Select(s => new PointF((float)s.X, (float)s.Y)).ToList();
        sel = shown.Count > 0 ? 0 : -1;
        Show();
    }

    void Show()
    {
        Card.IsVisible = sel >= 0;
        if (sel >= 0) { var s = shown[sel]; NameL.Text = s.Name; InfoL.Text = $"{s.Type} · {s.Km:0.0} km · {s.Hours}"; }
        map.Selected = sel; MapView.Invalidate();
    }

    void Chip_Clicked(object? s, EventArgs e) { filter = ((Button)s!).Text; Apply(); }
    void Search_Changed(object? s, TextChangedEventArgs e) => Apply();

    void Map_Touch(object? s, TouchEventArgs e)
    {
        var t = e.Touches[0]; int best = -1; double bd = 40;
        for (int i = 0; i < shown.Count; i++)
        {
            double d = Math.Sqrt(Math.Pow(shown[i].X * MapView.Width - t.X, 2) + Math.Pow(shown[i].Y * MapView.Height - t.Y, 2));
            if (d < bd) { bd = d; best = i; }
        }
        if (best >= 0) { sel = best; Show(); }
    }

    async void Directions_Clicked(object? s, EventArgs e)
    {
        if (sel < 0) return;
        var spot = shown[sel];
        try { await Microsoft.Maui.ApplicationModel.Map.Default.OpenAsync(spot.Lat, spot.Lon, new MapLaunchOptions { Name = spot.Name }); }
        catch (Exception) { await DisplayAlert("Directions", "No maps app available on this device.", "OK"); }
    }
}
