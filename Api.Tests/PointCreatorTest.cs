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


    // [Fact]
    // public void Test3()
    // {
    //     var p = new PointCreator(null);
    //     var start = new Coordinate(0, 0);
    //     var zero = new Coordinate(10, 0);
    //     var end = new Coordinate(0, 10);
    //     var list = p.getCurve2(start, zero, end);
    //     foreach (var l in list) _testOutputHelper.WriteLine($"x:{l.X},y:{l.Y}");
    // }
    //
    // [Fact]
    // public void TestSpline()
    // {
    //     var r = LinearSpline.Interpolate(
    //         new[] {0.0, 6.0, 10.0},
    //         new[] {0.0, 4.0, 10.0}
    //     );
    //     for (var i = 0.0; i <= 10.0; i = i + 1.0)
    //     {
    //         var y = r.Interpolate(i);
    //         _testOutputHelper.WriteLine($"x:{i},y:{y}");
    //     }
    // }

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