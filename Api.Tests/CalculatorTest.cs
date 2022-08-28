using System.Numerics;
using FluentAssertions;
using SheepHerding.Api.Entities;
using SheepHerding.Api.Helpers;

namespace SheepHerding.Api.Tests;
public class CalculatorTest
{
    [Fact]
    public void AngleToRadiansTest()
    {
        var result = Calculator.AngleToRadians(90.0);
        result.Should().BeApproximately(1.5708, 0.00001);
    }
    
    [Fact]
    public void AngleTest()
    {
        var result = Calculator.AngleInDegrees(0.0, 0.0, 3.0, 3.0);
        result.Should().BeApproximately(45.0, 0.00001);
    }
    
    [Fact]
    public void LenghtTest()
    {
        var result = Calculator.Length(0.0, 0.0, 3.0, 3.0);
        result.Should().BeApproximately(4.2426, 0.0001);
    }
    
    [Fact]
    public void CentroidTest()
    {
        var result = Calculator.Centroid(new List<Coordinate>
        {
            new (2, 2), new (2, 4), new (4, 2), new (4, 4)
        });
        result.x.Should().BeApproximately(3.0, 0.0001);
        result.y.Should().BeApproximately(3.0, 0.0001);
    }
    
    [Fact]
    public void VectorTest()
    {
        var v1 = new Vector2(2, 4);
        var result = Vector2.Divide(v1, 4);
        result.X.Should().BeApproximately(0.5f, 0.000001f);
        result.Y.Should().BeApproximately(1.0f, 0.000001f);
    }
}