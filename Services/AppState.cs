using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;

namespace WasteTracker;

/// <summary>App-wide state, persisted as JSON in the app's private data folder.</summary>
public static class AppState
{
    static readonly JsonSerializerOptions Opt = new() { WriteIndented = true, Converters = { new JsonStringEnumConverter() } };
    static string FilePath => Path.Combine(FileSystem.AppDataDirectory, "ecocycle.json");

    public static readonly WasteType[] Types =
    {
        new("Organic", "Food waste, garden waste", "🍃", "#E3F3E8"),
        new("Plastic", "Bottles, packaging, plastic bags", "🧴", "#E3EEFB"),
        new("Paper", "Newspapers, cardboard, books", "📄", "#FDF1D8"),
        new("Glass", "Bottles, jars, glass containers", "🍾", "#E0F4F4"),
        new("Metal", "Cans, metal packaging, foil", "🥫", "#ECEEF1"),
        new("E-Waste", "Electronic devices, batteries", "🔌", "#EFE3F8"),
    };

    public static readonly DropSpot[] Spots =
    {
        new("Recycling Center Melati", "Recycling Center", 1.2, "Open until 17:00", .30, .35, -6.2615, 106.8106),
        new("TPS Kebon Jeruk", "TPS", 0.8, "Open 24 hours", .62, .28, -6.2570, 106.8160),
        new("E-Waste Hub Sudirman", "E-Waste", 2.4, "Open until 18:00", .75, .62, -6.2700, 106.8230),
        new("Bank Sampah Asri", "Recycling Center", 3.1, "Open until 16:00", .22, .70, -6.2760, 106.8050),
        new("TPS Cempaka", "TPS", 1.9, "Open 24 hours", .50, .80, -6.2800, 106.8140),
    };

    public static AppData Data { get; private set; } = Load();

    static AppData Load()
    {
        try { if (File.Exists(FilePath)) return JsonSerializer.Deserialize<AppData>(File.ReadAllText(FilePath), Opt) ?? Seed(); }
        catch (JsonException) { }
        return Seed();
    }

    static AppData Seed()
    {
        var d = DateTime.Today; var m = new DateTime(d.Year, d.Month, 1);
        var data = new AppData();
        (string c, double kg, EntryKind k)[] rows =
        {
            ("Plastic", 2.5, EntryKind.Pickup), ("Organic", 1.8, EntryKind.DropOff), ("Paper", 3.2, EntryKind.Pickup),
            ("Glass", 2.6, EntryKind.Pickup), ("Metal", 2.3, EntryKind.DropOff),
        };
        for (int i = 0; i < rows.Length; i++)
            data.History.Insert(0, new WasteEntry { Date = m.AddDays(i), Category = rows[i].c, Kg = rows[i].kg, Kind = rows[i].k });
        return data;
    }

    public static void Save() => File.WriteAllText(FilePath, JsonSerializer.Serialize(Data, Opt));

    public static void Add(string category, double kg, EntryKind kind)
    {
        Data.History.Insert(0, new WasteEntry { Category = category, Kg = kg, Kind = kind });
        Data.Points += (int)Math.Round(kg * 10);
        Save();
    }

    /// <summary>Asks for a weight and logs it as a drop-off. Returns true if something was added.</summary>
    public static async Task<bool> PromptLog(Page page, WasteType t)
    {
        var s = await page.DisplayPromptAsync($"Log {t.Name}", "Weight in kg", "Add", "Cancel", "e.g. 1.5", keyboard: Keyboard.Numeric);
        if (double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out var kg) && kg > 0) { Add(t.Name, kg, EntryKind.DropOff); return true; }
        return false;
    }
}

public static class Ui
{
    public static void Chips(Layout host, string active)
    {
        foreach (var b in host.Children.OfType<Button>())
        {
            bool on = b.Text == active;
            b.BackgroundColor = on ? Color.FromArgb("#2E7D4F") : Color.FromArgb("#EEF2EF");
            b.TextColor = on ? Colors.White : Color.FromArgb("#1E2B24");
        }
    }
}
