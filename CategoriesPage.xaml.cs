namespace WasteTracker;

public partial class CategoriesPage : ContentPage
{
    public CategoriesPage() { InitializeComponent(); List.ItemsSource = AppState.Types; }
    async void Back_Clicked(object? s, EventArgs e) => await Shell.Current.GoToAsync("..");

    async void List_Selected(object? s, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is WasteType t)
        {
            List.SelectedItem = null;
            if (await AppState.PromptLog(this, t)) await DisplayAlert("Logged", $"{t.Name} added to your history.", "OK");
        }
    }
}
