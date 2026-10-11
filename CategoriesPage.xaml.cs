using System.Collections;
using System.Reflection;

namespace WasteTracker;

public partial class CategoriesPage : ContentPage
{
    public CategoriesPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        LoadCategories();
    }

    private void LoadCategories()
    {
        List.ItemsSource = AppState.Types;
    }

    private static IReadOnlyList<WasteType> GetWasteTypes()
    {
        var possibleValues = new List<object?>();

        try
        {
            object? data = AppState.Data;

            if (data != null)
            {
                foreach (string name in new[]
                {
                    "WasteTypes",
                    "Categories",
                    "Types"
                })
                {
                    PropertyInfo? property =
                        data.GetType().GetProperty(name);

                    if (property != null)
                    {
                        possibleValues.Add(
                            property.GetValue(data));
                    }
                }
            }

            foreach (string name in new[]
            {
                "WasteTypes",
                "Categories",
                "Types"
            })
            {
                PropertyInfo? property =
                    typeof(AppState).GetProperty(
                        name,
                        BindingFlags.Public |
                        BindingFlags.Static);

                if (property != null)
                {
                    possibleValues.Add(
                        property.GetValue(null));
                }
            }

            foreach (object? value in possibleValues)
            {
                if (value is IEnumerable enumerable)
                {
                    var result = enumerable
                        .Cast<object?>()
                        .OfType<WasteType>()
                        .ToList();

                    if (result.Count > 0)
                        return result;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Waste type lookup error: {ex}");
        }

        return Array.Empty<WasteType>();
    }

    private async void OnCategoryTapped(
        object? sender,
        TappedEventArgs e)
    {
        if (sender is not BindableObject bindable)
            return;

        if (bindable.BindingContext is not WasteType wasteType)
            return;

        try
        {
            await AppState.PromptLog(
                this,
                wasteType);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                $"Unable to record this waste item.\n\n{ex.Message}",
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