using Microsoft.Maui.Graphics;

namespace WasteTracker;

public partial class TrackPage : ContentPage
{
    private static readonly PointF[] Route =
    {
        new(0.85f, 0.15f),
        new(0.85f, 0.45f),
        new(0.55f, 0.45f),
        new(0.55f, 0.75f),
        new(0.20f, 0.75f)
    };

    private readonly TrackingDrawable _drawable = new();

    private float _progress;
    private bool _running;

    public TrackPage()
    {
        InitializeComponent();

        _drawable.Route = Route.ToList();
        _drawable.Home = Route[^1];
        _drawable.Truck = Route[0];

        MapView.Drawable = _drawable;

        RefreshTrackingUI();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _progress = 0;
        _running = true;

        RefreshTrackingUI();

        Dispatcher.StartTimer(
            TimeSpan.FromSeconds(1),
            () =>
            {
                if (!_running)
                    return false;

                _progress =
                    Math.Min(
                        1f,
                        _progress + 1f / 24f);

                _drawable.Truck =
                    GetPointAlongRoute(_progress);

                MapView.Invalidate();

                RefreshTrackingUI();

                return _progress < 1f;
            });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _running = false;
    }

    private static PointF GetPointAlongRoute(float progress)
    {
        if (progress <= 0)
            return Route[0];

        if (progress >= 1)
            return Route[^1];

        float[] lengths =
            new float[Route.Length - 1];

        float totalLength = 0;

        for (int i = 0; i < lengths.Length; i++)
        {
            float dx =
                Route[i + 1].X - Route[i].X;

            float dy =
                Route[i + 1].Y - Route[i].Y;

            lengths[i] =
                MathF.Sqrt(dx * dx + dy * dy);

            totalLength += lengths[i];
        }

        float distance =
            progress * totalLength;

        for (int i = 0; i < lengths.Length; i++)
        {
            if (distance <= lengths[i])
            {
                float ratio =
                    lengths[i] <= 0
                        ? 0
                        : distance / lengths[i];

                return new PointF(
                    Route[i].X +
                    (Route[i + 1].X - Route[i].X) * ratio,

                    Route[i].Y +
                    (Route[i + 1].Y - Route[i].Y) * ratio);
            }

            distance -= lengths[i];
        }

        return Route[^1];
    }

    private void RefreshTrackingUI()
    {
        bool arrived = _progress >= 1f;

        if (arrived)
        {
            EtaL.Text = "Arrived";
            StatusL.Text =
                "Driver has arrived at your address.";

            S1.TextColor =
                Color.FromArgb("#2E7D4F");

            S2.TextColor =
                Color.FromArgb("#2E7D4F");

            S3.TextColor =
                Color.FromArgb("#2E7D4F");

            return;
        }

        int minutes =
            Math.Max(
                1,
                (int)Math.Ceiling(
                    12 * (1 - _progress)));

        EtaL.Text =
            $"Arriving in {minutes} min";

        StatusL.Text =
            "Your waste collection vehicle is on the way.";

        S1.TextColor =
            Color.FromArgb("#2E7D4F");

        S2.TextColor =
            _progress > 0.02f
                ? Color.FromArgb("#2E7D4F")
                : Color.FromArgb("#9AA7A0");

        S3.TextColor =
            Color.FromArgb("#9AA7A0");
    }

    private async void Back_Clicked(
        object? sender,
        EventArgs e)
    {
        _running = false;

        await Shell.Current.GoToAsync("..");
    }
}


public sealed class TrackingDrawable : IDrawable
{
    public List<PointF> Route { get; set; } = new();

    public PointF Home { get; set; }

    public PointF Truck { get; set; }

    public void Draw(
        ICanvas canvas,
        RectF dirtyRect)
    {
        float width = dirtyRect.Width;
        float height = dirtyRect.Height;

        if (width <= 0 || height <= 0)
            return;

        canvas.FillColor =
            Color.FromArgb("#F5FAF6");

        canvas.FillRectangle(
            0,
            0,
            width,
            height);

        if (Route.Count < 2)
            return;

        PointF ToScreen(PointF point)
        {
            return new PointF(
                point.X * width,
                point.Y * height);
        }

        // Route
        canvas.StrokeColor =
            Color.FromArgb("#B9DEC5");

        canvas.StrokeSize = 12;

        for (int i = 0; i < Route.Count - 1; i++)
        {
            canvas.DrawLine(
                ToScreen(Route[i]),
                ToScreen(Route[i + 1]));
        }

        // Inner route
        canvas.StrokeColor =
            Color.FromArgb("#2E7D4F");

        canvas.StrokeSize = 4;

        for (int i = 0; i < Route.Count - 1; i++)
        {
            canvas.DrawLine(
                ToScreen(Route[i]),
                ToScreen(Route[i + 1]));
        }

        // Home marker
        PointF home =
            ToScreen(Home);

        canvas.FillColor =
            Color.FromArgb("#1F5D38");

        canvas.FillCircle(
            home.X,
            home.Y,
            13);

        canvas.FillColor =
            Colors.White;

        canvas.FillCircle(
            home.X,
            home.Y,
            5);

        // Truck
        PointF truck =
            ToScreen(Truck);

        canvas.FillColor =
            Color.FromArgb("#2E7D4F");

        canvas.FillRoundedRectangle(
            truck.X - 17,
            truck.Y - 12,
            30,
            21,
            5);

        canvas.FillColor =
            Color.FromArgb("#66A87E");

        canvas.FillRoundedRectangle(
            truck.X + 9,
            truck.Y - 7,
            13,
            16,
            4);

        canvas.FillColor =
            Color.FromArgb("#25342B");

        canvas.FillCircle(
            truck.X - 8,
            truck.Y + 11,
            4);

        canvas.FillCircle(
            truck.X + 14,
            truck.Y + 11,
            4);

        canvas.FillColor =
            Colors.White;

        canvas.FillRectangle(
            truck.X + 12,
            truck.Y - 4,
            7,
            6);
    }
}