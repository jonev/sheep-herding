using SheepHerding.Api.Entities;

namespace SheepHerding.Api.Helpers;

public static class Clustering
{
    public static List<List<Point>> Cluster(IList<Point> input, double limit)
    {
        var groups = new List<List<Point>> {new ()};

        for (int i = 0; i < input.Count; i++)
        {
            foreach (var group in groups)
            {
                if (group.Count == 0)
                {
                    group.Add(input[i]);
                    goto FirstElement;
                }

                foreach (var groupElement in group)
                {
                    if (Math.Abs(input[i].Position.X - groupElement.Position.X) <= limit 
                        && Math.Abs(input[i].Position.Y - groupElement.Position.Y) <= limit)
                    {
                        group.Add(input[i]);
                        goto NotFirstElement;
                    }
                }
            }

            groups.Add(new List<Point>{input[i]});
            FirstElement: 
            NotFirstElement:
            NoOperation();
        }
        
        return groups;
    }

    private static void NoOperation()
    {
        
    }
}