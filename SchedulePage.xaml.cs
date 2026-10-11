namespace WasteTracker;

public partial class SchedulePage : ContentPage
{
    private readonly string[] _timeSlots =
    {
        "Morning (8:00 AM - 12:00 PM)",
        "Afternoon (1:00 PM - 5:00 PM)"
    };

    public SchedulePage()
    {
        InitializeComponent();

        PickupDatePicker.MinimumDate = DateTime.Today;
        PickupDatePicker.Date = DateTime.Today;

        TimeSlotPicker.ItemsSource = _timeSlots;

        PickupDatePicker.DateSelected +=
            OnScheduleValueChanged;

        TimeSlotPicker.SelectedIndexChanged +=
            OnScheduleValueChanged;

        UpdateSummary();
    }

    private void OnScheduleValueChanged(
        object? sender,
        EventArgs e)
    {
        UpdateSummary();
    }

    private void UpdateSummary()
    {
        DateTime selectedDate =
            PickupDatePicker.Date ?? DateTime.Today;

        string slot =
            TimeSlotPicker.SelectedItem?.ToString()
            ?? "No time selected";

        SummaryLabel.Text =
            $"{selectedDate:dddd, d MMMM yyyy}\n{slot}";
    }

   private async void OnConfirmScheduleClicked(
    object? sender,
    EventArgs e)
    {
        try
        {
            DateTime selectedDate =
                PickupDatePicker.Date ?? DateTime.Today;

            if (selectedDate.Date < DateTime.Today)
            {
                await DisplayAlertAsync(
                    "Invalid Date",
                    "Please choose today or a future date.",
                    "OK");
                return;
            }

            if (TimeSlotPicker.SelectedIndex < 0)
            {
                await DisplayAlertAsync(
                    "Choose a Time",
                    "Please select a pickup time slot.",
                    "OK");
                return;
            }

            if (AppState.Data == null)
            {
                await DisplayAlertAsync(
                    "Error",
                    "Application data is unavailable.",
                    "OK");
                return;
            }

            string selectedSlot =
                TimeSlotPicker.SelectedItem?.ToString()
                ?? string.Empty;

            AppState.Data.Next = new ScheduledPickup
            {
                Date = selectedDate,
                Slot = selectedSlot
            };

            await DisplayAlertAsync(
                "Pickup Scheduled",
                $"Your pickup is scheduled for:\n\n" +
                $"{selectedDate:dddd, d MMMM yyyy}\n" +
                selectedSlot,
                "OK");

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Scheduling Error",
                ex.Message,
                "OK");
        }
    }
    private async void Back_Clicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}