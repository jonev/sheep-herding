using System.Text;

namespace SheepHerding.Api.Entities;

public class Coordinate
{
    public double X { get; private set; } = 0;
    public double Y { get; private set; } = 0;

    public void Update(double x, double y)
    {
        X = x;
        Y = y;
    }
    
    public void Update(Coordinate next)
    {
        X = next.X;
        Y = next.Y;
    }

    public Coordinate(double x, double y)
    {
        X = x;
        Y = y;
    }
    
    public Coordinate()
    {
    }

    public Point ToPoint()
    {
        return new Point(0, 0, 0, X, Y);
    }

    public override string ToString()
    {
        return $"{nameof(X)}: {X}, {nameof(Y)}: {Y}";
    }
}

public class AckableCoordinate : Coordinate
{
    public int PathIndex { get; set; }
    public bool Accessed { get; set; }

    public AckableCoordinate(int pathIndex, double x, double y) : base(x, y)
    {
        PathIndex = pathIndex;
    }

    public void Ack()
    {
        Accessed = true;
    }
}

public static class CoordinatePrinter
{
    public static string ToString(IList<Coordinate>? coordinates)
    {
        if (coordinates is null || coordinates.Count < 1) return "";
        var sb = new StringBuilder();
        foreach (var coordinate in coordinates)
        {
            sb.Append($"{coordinate.X},{coordinate.Y};");
        }

        return sb.ToString();
    }
}