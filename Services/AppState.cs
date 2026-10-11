using System.Net.Http.Json;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;
namespace WasteTracker;

public class UserData
{
    public string Name { get; set; } = string.Empty;
    public List<WasteLog> History { get; set; } = new();
    internal ScheduledPickup? Next { get; set; }
}

public class WasteLog
{
    public DateTime Date { get; set; }
    public double Kg { get; set; }
    public string Category { get; set; } = string.Empty;
}

public class ScheduledPickup
{
    public DateTime? Date { get; set; }
    public string Slot { get; set; } = string.Empty;
}

public static class AppState
{
    public static List<WasteType> Types { get; set; } = new()
    {
        new WasteType("Recyclable", "♻️", "Green", "Recyclable materials"),
        new WasteType("Organic", "🌱", "Brown", "Organic waste"),
        new WasteType("Hazardous", "⚠️", "Red", "Hazardous waste"),
        new WasteType("General Waste", "🗑️", "Gray", "General waste")
    };

    private static readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("https://wastetracker-api.onrender.com")
    };

    public static UserData? _data;
    public static UserData Data
    {
        get
        {
            _data ??= new UserData();
            string savedName = Preferences.Get("LoggedInUsername", Preferences.Get("Username", string.Empty));
            if (!string.IsNullOrWhiteSpace(savedName) && savedName != "Eco Tracker")
            {
                _data.Name = savedName;
            }
            return _data;
        }
        set => _data = value;
    }

    public static async Task<bool> PromptLog(Page page, WasteType wasteType)
    {
        string result = await page.DisplayPromptAsync("Log Waste", $"Enter weight in Kg for {wasteType.Name}:", keyboard: Keyboard.Numeric);
        
        if (double.TryParse(result, out double kg) && kg > 0)
        {
            string username = Preferences.Get("LoggedInUsername", Preferences.Get("Username", string.Empty));

            var logPayload = new
            {
                Username = username,
                Kg = kg,
                Category = wasteType.Name
            };

            try
            {
                // Send log directly to Render database
                var response = await _httpClient.PostAsJsonAsync("/api/waste/log", logPayload);

                if (response.IsSuccessStatusCode)
                {
                    Data.History.Add(new WasteLog
                    {
                        Date = DateTime.Now,
                        Kg = kg,
                        Category = wasteType.Name
                    });

                    await page.DisplayAlert("Success", $"Logged {kg} kg of {wasteType.Name} to your account!", "OK");
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LOG ERROR]: {ex.Message}");
            }
        }
        return false;
    }
    
}