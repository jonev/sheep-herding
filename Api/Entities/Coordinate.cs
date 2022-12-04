using System.Text;

namespace SheepHerding.Api.Entities;

public class Coordinate
{
    public Coordinate(double x, double y)
    {
        X = x;
        Y = y;
    }

    public Coordinate(Coordinate coordinate)
    {
        X = coordinate.X;
        Y = coordinate.Y;
    }

    public Coordinate()
    {
    }

    public double X { get; private set; }
    public double Y { get; private set; }

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

    public Point ToPoint()
    {
        return new Point(0, X, Y);
    }

    public override string ToString()
    {
        return $"{nameof(X)}: {X}, {nameof(Y)}: {Y}";
    }
}

public class AckableCoordinate : Coordinate
{
    public AckableCoordinate(int pathIndex, double x, double y, bool isPartOfCurve = false) : base(x, y)
    {
        PathIndex = pathIndex;
        IsPartOfCurve = isPartOfCurve;
    }

    public int PathIndex { get; set; }
    public bool Accessed { get; set; }
    public bool IsPartOfCurve { get; set; }

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
        sb.Append($"{coordinates[0].X},{coordinates[0].Y}");
        foreach (var coordinate in coordinates.Skip(1)) sb.Append($";{coordinate.X},{coordinate.Y}");

        return sb.ToString();
    }
}