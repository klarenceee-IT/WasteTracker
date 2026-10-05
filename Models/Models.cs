using System.Text.Json.Serialization;
namespace WasteTracker;

public enum EntryKind { Pickup, DropOff }

public record WasteType(string Name, string Desc, string Icon, string Tint)
{
    public Color TintColor => Color.FromArgb(Tint);
}

public class WasteEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Date { get; set; } = DateTime.Today;
    public string Category { get; set; } = "";
    public double Kg { get; set; }
    public EntryKind Kind { get; set; }

    [JsonIgnore] public WasteType Type => AppState.Types.First(t => t.Name == Category);
    [JsonIgnore] public string Icon => Type.Icon;
    [JsonIgnore] public Color TintColor => Type.TintColor;
    [JsonIgnore] public string KgText => $"{Kg:0.##} kg";
    [JsonIgnore] public string Sub => $"{Date:dd MMM yyyy} · {(Kind == EntryKind.Pickup ? "Pickup" : "Drop Off")}";
}

public class Pickup
{
    public DateTime Date { get; set; }
    public string Slot { get; set; } = "";
    public string Address { get; set; } = "";
    public string Notes { get; set; } = "";
}

public class AppData
{
    public string Name { get; set; } = "Ahmad Fauzi";
    public string Email { get; set; } = "ahmad.fauzi@email.com";
    public string Address { get; set; } = "Jl. Mawar No. 12, Jakarta Selatan";
    public bool Notify { get; set; } = true;
    public int Points { get; set; } = 320;
    public Pickup? Next { get; set; }
    public List<WasteEntry> History { get; set; } = new();
}

public record DropSpot(string Name, string Type, double Km, string Hours, double X, double Y, double Lat, double Lon);
public record MenuRow(string Icon, string Title, string Sub);
