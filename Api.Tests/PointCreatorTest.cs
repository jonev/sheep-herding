using SheepHerding.Api.Entities;
using SheepHerding.Api.Helpers;
using Xunit.Abstractions;

namespace SheepHerding.Api.Tests;

public class PointCreatorTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public PointCreatorTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void TestBezier()
    {
        var p = new PointCreator(null);
        var start = new Coordinate(0, 0);
        var zero = new Coordinate(10, 0);
        var end = new Coordinate(10, 10);
        var list = p.Bezier(start, zero, end, 1);
        foreach (var l in list) _testOutputHelper.WriteLine($"i:{l.PathIndex},x:{l.X},y:{l.Y}");
    }
}