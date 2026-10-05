namespace WasteTracker;

public partial class SchedulePage : ContentPage
{
    static readonly string[] SlotNames = { "08:00 - 10:00", "10:00 - 11:00", "13:00 - 14:00", "15:00 - 17:00" };
    DateTime month = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    DateTime picked = DateTime.Today.AddDays(1);
    int slot;

    public SchedulePage()
    {
        InitializeComponent();
        for (int i = 0; i < 7; i++) Cal.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        AddrPicker.ItemsSource = new[] { $"Home – {AppState.Data.Address}", "Office – Jl. Sudirman No. 45, Jakarta Pusat" };
        AddrPicker.SelectedIndex = 0;
        BuildCal(); BuildSlots();
    }

    void BuildCal()
    {
        Cal.Children.Clear();
        MonthL.Text = month.ToString("MMMM yyyy");
        string[] hd = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
        for (int c = 0; c < 7; c++)
            Cal.Add(new Label { Text = hd[c], FontSize = 11, HorizontalTextAlignment = TextAlignment.Center, TextColor = Color.FromArgb("#6C7A72") }, c, 0);
        int off = (int)month.DayOfWeek;
        for (int d = 1; d <= DateTime.DaysInMonth(month.Year, month.Month); d++)
        {
            var date = new DateTime(month.Year, month.Month, d); int idx = off + d - 1;
            bool sel = date == picked.Date;
            var b = new Button
            {
                Text = d.ToString(), Padding = new Thickness(0), WidthRequest = 38, HeightRequest = 38, CornerRadius = 19, FontSize = 13,
                IsEnabled = date >= DateTime.Today,
                BackgroundColor = sel ? Color.FromArgb("#2E7D4F") : Colors.Transparent,
                TextColor = sel ? Colors.White : Color.FromArgb("#1E2B24"),
            };
            b.Clicked += (_, _) => { picked = date; BuildCal(); };
            Cal.Add(b, idx % 7, 1 + idx / 7);
        }
    }

    void BuildSlots()
    {
        Slots.Children.Clear();
        for (int i = 0; i < SlotNames.Length; i++)
        {
            int n = i; bool on = n == slot;
            var b = new Button
            {
                Text = SlotNames[i], CornerRadius = 12, HeightRequest = 44, FontSize = 13, BorderWidth = 1,
                BorderColor = on ? Color.FromArgb("#2E7D4F") : Color.FromArgb("#E3E8E5"),
                BackgroundColor = on ? Color.FromArgb("#E6F2EA") : Colors.White, TextColor = Color.FromArgb("#1E2B24"),
            };
            b.Clicked += (_, _) => { slot = n; BuildSlots(); };
            Slots.Add(b, i % 2, i / 2);
        }
    }

    void Prev_Clicked(object? s, EventArgs e) { month = month.AddMonths(-1); BuildCal(); }
    void Next_Clicked(object? s, EventArgs e) { month = month.AddMonths(1); BuildCal(); }
    async void Back_Clicked(object? s, EventArgs e) => await Shell.Current.GoToAsync("..");

    async void Confirm_Clicked(object? s, EventArgs e)
    {
        AppState.Data.Next = new Pickup { Date = picked, Slot = SlotNames[slot], Address = AddrPicker.SelectedItem?.ToString() ?? "", Notes = NotesE.Text ?? "" };
        AppState.Data.Points += 10;
        AppState.Save();
        await DisplayAlert("Pickup scheduled", $"{picked:dddd, d MMMM}\n{SlotNames[slot]}\n+10 Green Points", "OK");
        await Shell.Current.GoToAsync("..");
    }
}
