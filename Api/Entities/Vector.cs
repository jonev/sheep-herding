using System.Numerics;
using System.Text;
using SheepHerding.Api.Helpers;

namespace SheepHerding.Api.Entities;

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
        var x2 = (start.X + (length * Math.Cos(angle)));
        var y2 = (start.Y + (length * Math.Sin(angle)));
        End.Update(x2, y2);
    }

    public void Update(Coordinate start, Coordinate end)
    {
        Start.Update(start);
        End.Update(end);
        Angle = Calculator.AngleInRadians(start.X, start.Y, end.X, end.Y);
        Lenght = Calculator.Length(start.X, start.Y, end.X, end.Y);
    }

    public override string ToString()
    {
        return $"{nameof(Start)}: {Start}, {nameof(End)}: {End}, {nameof(Angle)}: {Angle}, {nameof(Lenght)}: {Lenght}";
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
    
    public static string ToString(IList<Sheep> sheeps)
    {
        var sb = new StringBuilder();
        foreach (var sheep in sheeps)
        {
            sb.Append($"{sheep.Position.X},{sheep.Position.Y},{sheep.Position.X + sheep.Force.X},{sheep.Position.Y + sheep.Force.Y};");
        }

        return sb.ToString();
    }
}