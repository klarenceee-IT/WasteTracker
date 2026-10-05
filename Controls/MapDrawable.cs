using Microsoft.Maui.Graphics;
namespace WasteTracker;

/// <summary>A stylised offline map: blocks, roads, route, pins. Coordinates are 0..1.</summary>
public class MapDrawable : IDrawable
{
    public List<PointF> Route { get; set; } = new();
    public List<PointF> Pins { get; set; } = new();
    public int Selected { get; set; } = -1;
    public PointF? Truck { get; set; }
    public PointF? Home { get; set; }

    public void Draw(ICanvas c, RectF r)
    {
        c.FillColor = Color.FromArgb("#EAF1EC"); c.FillRectangle(r);
        c.FillColor = Color.FromArgb("#DCE8E0");
        for (int i = 0; i < 5; i++)
            for (int j = 0; j < 4; j++)
                c.FillRoundedRectangle(r.Width * (.03f + i * .2f), r.Height * (.04f + j * .25f), r.Width * .15f, r.Height * .19f, 6);
        c.StrokeColor = Colors.White; c.StrokeSize = 9;
        for (int i = 0; i < 4; i++) c.DrawLine(r.Width * (.19f + i * .2f), 0, r.Width * (.19f + i * .2f), r.Height);
        for (int j = 0; j < 3; j++) c.DrawLine(0, r.Height * (.245f + j * .25f), r.Width, r.Height * (.245f + j * .25f));

        if (Route.Count > 1)
        {
            var p = new PathF(Route[0].X * r.Width, Route[0].Y * r.Height);
            foreach (var q in Route.Skip(1)) p.LineTo(q.X * r.Width, q.Y * r.Height);
            c.StrokeColor = Color.FromArgb("#2E7D4F"); c.StrokeSize = 5; c.StrokeLineJoin = LineJoin.Round;
            c.DrawPath(p);
        }

        for (int i = 0; i < Pins.Count; i++)
        {
            float x = Pins[i].X * r.Width, y = Pins[i].Y * r.Height; bool sel = i == Selected;
            c.FillColor = sel ? Color.FromArgb("#1B5E3A") : Color.FromArgb("#2E7D4F");
            c.FillCircle(x, y, sel ? 13 : 9);
            c.StrokeColor = Colors.White; c.StrokeSize = 3; c.DrawCircle(x, y, sel ? 13 : 9);
        }

        c.FontSize = 26;
        if (Home is { } h) c.DrawString("🏠", h.X * r.Width - 20, h.Y * r.Height - 20, 40, 40, HorizontalAlignment.Center, VerticalAlignment.Center);
        if (Truck is { } t) c.DrawString("🚚", t.X * r.Width - 20, t.Y * r.Height - 20, 40, 40, HorizontalAlignment.Center, VerticalAlignment.Center);
    }
}
