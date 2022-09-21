using System.Numerics;
using System.Text;

namespace SheepHerding.Api.Entities;
public static class VectorPrinter
{
    public static string ToString(IList<Point> points)
    {
        var sb = new StringBuilder();
        foreach (var sheep in points)
        {
            sb.Append($"{sheep.Position.X},{sheep.Position.Y},{sheep.Position.X + sheep.Force.X},{sheep.Position.Y + sheep.Force.Y};");
        }

        return sb.ToString();
    }
}