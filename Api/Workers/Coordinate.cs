using System.Text;

namespace SignalRDraw.Workers;

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

    public override string ToString()
    {
        return $"{nameof(X)}: {X}, {nameof(Y)}: {Y}";
    }
}

public static class CoordinatePrinter
{
    public static string ToString(IList<Coordinate> coordinates)
    {
        var sb = new StringBuilder();
        foreach (var coordinate in coordinates)
        {
            sb.Append($"{coordinate.X},{coordinate.Y};");
        }

        return sb.ToString();
    }
}