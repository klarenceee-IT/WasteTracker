namespace WasteTracker;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
        BindableLayout.SetItemsSource(Cats, AppState.Types);
    }

    protected override void OnAppearing() { base.OnAppearing(); Refresh(); }

    void Refresh()
    {
        var d = AppState.Data; var now = DateTime.Today;
        Hello.Text = $"Hello, {d.Name.Split(' ')[0]}";
        Total.Text = $"{d.History.Where(e => e.Date.Year == now.Year && e.Date.Month == now.Month).Sum(e => e.Kg):0.#} kg";
        NextL.Text = d.Next is { } n ? $"{n.Date:ddd, d MMM} · {n.Slot}" : "Nothing scheduled yet";
    }

    async void Quick_Clicked(object? s, EventArgs e)
    {
        switch (((Button)s!).ClassId)
        {
            case "schedule": await Shell.Current.GoToAsync("schedule"); break;
            case "pickup": await Shell.Current.GoToAsync("track"); break;
            case "dropoff": await Shell.Current.GoToAsync("//main/map"); break;
            default: await DisplayAlert("Quick tutorial", "1. Sort by category\n2. Rinse containers\n3. Keep hazardous items separate\n4. Schedule a pickup or drop off", "Got it"); break;
        }
    }

    async void SeeAll_Tapped(object? s, TappedEventArgs e) => await Shell.Current.GoToAsync("categories");
    async void Next_Tapped(object? s, TappedEventArgs e) => await Shell.Current.GoToAsync("track");

    async void Cat_Tapped(object? s, TappedEventArgs e)
    {
        if (((BindableObject)s!).BindingContext is WasteType t && await AppState.PromptLog(this, t)) Refresh();
    }
}
