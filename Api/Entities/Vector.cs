using System.Text;

namespace SignalRDraw.Workers;

public class Vector
{
    public double Angle { get; private set; }
    public double Lenght { get; private set; }
    internal Coordinate Start = new ();
    internal Coordinate End = new ();

    public void Set(Coordinate start, double angle, double length)
    {
        Start.Update(start);
        Angle = angle;
        Lenght = length;
        var x2 = (start.X + (length * Math.Cos(AngleToRadians(angle))));
        var y2 = (start.Y + (length * Math.Sin(AngleToRadians(angle))));
        End.Update(x2, y2);
    }

    public void Update(Coordinate start, Coordinate end)
    {
        Start.Update(start);
        End.Update(end);
        Angle = Math.Atan2(end.Y - start.Y, end.X - start.X) * 180.0 / Math.PI;
        Lenght = Math.Sqrt((Math.Pow(end.X - start.X, 2) + Math.Pow(end.Y - start.Y, 2)));
    }

    public override string ToString()
    {
        return $"{nameof(Start)}: {Start}, {nameof(End)}: {End}, {nameof(Angle)}: {Angle}, {nameof(Lenght)}: {Lenght}";
    }
    
    internal double AngleToRadians(double angle)
    {
        return (Math.PI / 180) * angle;
    }
}

public static class VectorPrinter
{
    public static string ToString(IList<Vector> vectors)
    {
        var sb = new StringBuilder();
        foreach (var vector in vectors)
        {
            sb.Append($"{vector.Start.X},{vector.Start.Y},{vector.End.X},{vector.End.Y};");
        }

        return sb.ToString();
    }
}