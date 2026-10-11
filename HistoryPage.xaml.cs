namespace WasteTracker;

public partial class HistoryPage : ContentPage
{
    private string _currentFilter = "All";

    public HistoryPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadHistory(_currentFilter);
    }

    private void Chip_Clicked(object? sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        _currentFilter = button.Text ?? "All";

        UpdateChipAppearance(button);
        LoadHistory(_currentFilter);
    }

    private void UpdateChipAppearance(Button selected)
    {
        foreach (var child in Chips.Children)
        {
            if (child is Button button)
            {
                bool isSelected = button == selected;

                button.BackgroundColor = isSelected
                    ? Color.FromArgb("#2E7D4F")
                    : Color.FromArgb("#E5ECE8");

                button.TextColor = isSelected
                    ? Colors.White
                    : Color.FromArgb("#385145");
            }
        }
    }

    private void LoadHistory(string filter)
    {
        try
        {
            var history = AppState.Data.History.ToList();

            DateTime today = DateTime.Today;

            IEnumerable<WasteLog> filtered = history;

            if (filter == "This Month")
            {
                filtered = history.Where(x =>
                    x.Date.Year == today.Year &&
                    x.Date.Month == today.Month);
            }
            else if (filter == "Older")
            {
                filtered = history.Where(x =>
                    x.Date.Year != today.Year ||
                    x.Date.Month != today.Month);
            }

            var result = filtered
                .OrderByDescending(x => x.Date)
                .ToList();

            List.ItemsSource = result;

            RecordCountLabel.Text = result.Count.ToString();

            double totalKg = result.Sum(x => x.Kg);

            TotalWeightLabel.Text = $"{totalKg:0.#} kg";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"History loading error: {ex}");

            List.ItemsSource = Array.Empty<WasteLog>();

            RecordCountLabel.Text = "0";
            TotalWeightLabel.Text = "0 kg";
        }
    }
}