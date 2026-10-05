namespace WasteTracker;

public partial class ProfilePage : ContentPage
{
    public ProfilePage() { InitializeComponent(); }
    protected override void OnAppearing() { base.OnAppearing(); Refresh(); }

    void Refresh()
    {
        var d = AppState.Data;
        Initials.Text = string.Concat(d.Name.Split(' ').Take(2).Select(w => w[0]));
        NameL.Text = d.Name; EmailL.Text = d.Email; PointsL.Text = $"{d.Points} pts";
        BindableLayout.SetItemsSource(Menu, new[]
        {
            new MenuRow("📍", "My Address", d.Address),
            new MenuRow("🔔", "Notifications", d.Notify ? "On — collection reminders, updates" : "Off"),
            new MenuRow("💳", "Payment Method", "Manage your payment methods"),
            new MenuRow("❓", "Help & Support", "FAQ, contact us"),
            new MenuRow("ℹ️", "About", "App version 1.0.0"),
        });
    }

    async void Row_Tapped(object? s, TappedEventArgs e)
    {
        if (((BindableObject)s!).BindingContext is not MenuRow row) return;
        switch (row.Title)
        {
            case "My Address":
                var a = await DisplayPromptAsync("My Address", "Pickup address", "Save", "Cancel", initialValue: AppState.Data.Address);
                if (!string.IsNullOrWhiteSpace(a)) { AppState.Data.Address = a.Trim(); AppState.Save(); }
                break;
            case "Notifications":
                AppState.Data.Notify = !AppState.Data.Notify; AppState.Save(); break;
            default:
                await DisplayAlert(row.Title, "Coming soon.", "OK"); break;
        }
        Refresh();
    }

    async void Logout_Clicked(object? s, EventArgs e) => await Shell.Current.GoToAsync("//welcome");
}
