using FluentAssertions;
using SheepHerding.Api.Entities;
using SheepHerding.Api.Helpers;

namespace SheepHerding.Api.Tests;

public class ClusteringTest
{
    class TestPoint : Point
    {
        public TestPoint(int id, double x, double y) : base(0, 0, id, x, y)
        {
        }
    }
    [Fact]
    public void Cluster1Test()
    {
        var list = new List<Point>
        {
            new TestPoint(1, 0, 0),
            new TestPoint(2, 1, 1),
            new TestPoint(3, 3, 3),
            new TestPoint(4, 10, 10),
            new TestPoint(5, 11, 11)
        };

        var result = Clustering.Cluster(list, 2.0);
        result.Count.Should().Be(2);
        result[0].Count.Should().Be(3);
        result[1].Count.Should().Be(2);
    }
    
    [Fact]
    public void Cluster2Test()
    {
        var list = new List<Point>
        {
            new TestPoint(1, 0, 0),
            new TestPoint(4, 10, 10),
            new TestPoint(5, 11, 11),
            new TestPoint(5, 100, 100),
            new TestPoint(5, 13, 13),
            new TestPoint(5, 102, 102),
        };

        var result = Clustering.Cluster(list, 2.0);
        result.Count.Should().Be(3);
        result[0].Count.Should().Be(1);
        result[1].Count.Should().Be(3);
        result[2].Count.Should().Be(2);
    }
}