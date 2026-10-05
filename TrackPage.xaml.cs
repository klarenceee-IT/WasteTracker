namespace WasteTracker;

public partial class TrackPage : ContentPage
{
    static readonly PointF[] Path = { new(.85f, .15f), new(.85f, .5f), new(.5f, .5f), new(.5f, .85f), new(.15f, .85f) };
    readonly MapDrawable map = new();
    float t; bool running;

    public TrackPage()
    {
        InitializeComponent();
        map.Route = Path.ToList(); map.Home = Path[^1]; map.Truck = Path[0];
        MapView.Drawable = map;
        Refresh();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        t = 0; running = true; Refresh();
        Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            if (!running) return false;
            t = Math.Min(1, t + 1f / 24f); // demo speed: ~24 s for the 12-minute trip
            map.Truck = At(t); MapView.Invalidate(); Refresh();
            return t < 1;
        });
    }

    protected override void OnDisappearing() { base.OnDisappearing(); running = false; }

    static PointF At(float t)
    {
        var len = new float[Path.Length - 1]; float total = 0;
        for (int i = 0; i < len.Length; i++) { len[i] = MathF.Sqrt(MathF.Pow(Path[i + 1].X - Path[i].X, 2) + MathF.Pow(Path[i + 1].Y - Path[i].Y, 2)); total += len[i]; }
        float d = t * total;
        for (int i = 0; i < len.Length; i++)
        {
            if (d <= len[i] || i == len.Length - 1)
            {
                float k = len[i] == 0 ? 1 : Math.Min(1, d / len[i]);
                return new PointF(Path[i].X + (Path[i + 1].X - Path[i].X) * k, Path[i].Y + (Path[i + 1].Y - Path[i].Y) * k);
            }
            d -= len[i];
        }
        return Path[^1];
    }

    void Refresh()
    {
        bool arrived = t >= 1;
        EtaL.Text = arrived ? "Arrived" : $"Arriving in {(int)Math.Ceiling(12 * (1 - t))} min";
        StatusL.Text = arrived ? "Driver has arrived at your address" : "On the way to your address";
        var on = Color.FromArgb("#2E7D4F"); var off = Color.FromArgb("#9AA7A0");
        S1.TextColor = on; S2.TextColor = on; S3.TextColor = arrived ? on : off;
    }

    async void Back_Clicked(object? s, EventArgs e) => await Shell.Current.GoToAsync("..");
}
