using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
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
        private static readonly HttpClient _httpClient = new()
        {
            BaseAddress = new Uri("https://wastetracker-api.onrender.com")
        };

        public static List<WasteType> Types { get; set; } = new()
        {
            new WasteType("Recyclable", "♻️", "Green", "Recyclable materials"),
            new WasteType("Organic", "🌱", "Brown", "Organic waste"),
            new WasteType("Hazardous", "⚠️", "Red", "Hazardous waste"),
            new WasteType("General Waste", "🗑️", "Gray", "General waste")
        };

        public static UserData? _data;
        public static UserData Data
        {
            get
            {
                if (_data == null)
                {
                    _data = new UserData();
                }

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
            double roundedKg = Math.Round(kg, 2);

            var logPayload = new
            {
                username = Data.Name,
                kg = roundedKg,
                category = wasteType.Name
            };

            try
            {
                string json = JsonSerializer.Serialize(logPayload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Post directly to PostgreSQL backend
                HttpResponseMessage response = await _httpClient.PostAsync("/api/waste/log", content);

                if (response.IsSuccessStatusCode)
                {
                    Data.History.Add(new WasteLog
                    {
                        Date = DateTime.Now,
                        Kg = kg,
                        Category = wasteType.Name
                    });

                    await page.DisplayAlert("Success", $"Logged {kg} kg of {wasteType.Name}!", "OK");
                    return true;
                }
                else
                {
                    string err = await response.Content.ReadAsStringAsync();
                    await page.DisplayAlert("Error Logging Waste", string.IsNullOrWhiteSpace(err) ? "Server rejected the log entry." : err, "OK");
                }
            }
            catch (Exception ex)
            {
                await page.DisplayAlert("Connection Error", $"Failed to post log: {ex.Message}", "OK");
            }
        }
        return false;
    }
    
}