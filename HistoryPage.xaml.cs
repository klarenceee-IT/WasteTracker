namespace WasteTracker;

public partial class HistoryPage : ContentPage
{
    string filter = "All";
    public HistoryPage() { InitializeComponent(); }
    protected override void OnAppearing() { base.OnAppearing(); Apply(); }

    void Chip_Clicked(object? s, EventArgs e) { filter = ((Button)s!).Text; Apply(); }

    void Apply()
    {
        Ui.Chips(Chips, filter);
        var q = AppState.Data.History.AsEnumerable();
        if (filter == "Pickup") q = q.Where(x => x.Kind == EntryKind.Pickup);
        if (filter == "Drop Off") q = q.Where(x => x.Kind == EntryKind.DropOff);
        List.ItemsSource = q.OrderByDescending(x => x.Date).ToList();
    }
}
