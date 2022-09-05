using System.Numerics;
using FluentAssertions;
using SheepHerding.Api.Entities;
using SheepHerding.Api.Helpers;

namespace SheepHerding.Api.Tests;
public class CalculatorTest
{
    private readonly List<Coordinate> _path = new List<Coordinate> {new(100, 100), new(800, 100), new(800, 300), new(200, 300), new(200, 600), new(800, 600), new(950, 850)};

    [Fact]
    public void AngleToRadiansTest()
    {
        var result = Calculator.DegreesToRadians(90.0);
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
    
    [Fact]
    public void FlipVector1Test()
    {
        var v1 = new Vector2(4, 0);
        var result = Calculator.FlipLength(v1, 5.0);
        result.Length().Should().BeApproximately(1.0f, 0.001f);
    }
    
    [Fact]
    public void FlipVector2Test()
    {
        var v1 = new Vector2(0, 4);
        var result = Calculator.FlipLength(v1, 5.0);
        result.Length().Should().BeApproximately(1.0f, 0.001f);
    }
    
    [Fact]
    public void FlipVector3Test()
    {
        var v1 = new Vector2(4, 4);
        var result = Calculator.FlipLength(v1, 6.0);
        result.Length().Should().BeApproximately(0.343145f, 0.001f);
    }
    
    [Fact]
    public void RotateVector1Test()
    {
        var v1 = new Vector2(4, 0);
        var result = Calculator.RotateVector(v1, Math.PI);
        result.X.Should().BeApproximately(-4, 0.001f);
        result.Y.Should().BeApproximately(0, 0.001f);
    }
    
    [Fact]
    public void RotateVector2Test()
    {
        var v1 = new Vector2(4, 0);
        var result = Calculator.RotateVector(v1, Math.PI/2);
        result.X.Should().BeApproximately(0, 0.001f);
        result.Y.Should().BeApproximately(4, 0.001f);
    }
    
    [Fact]
    public void RotateVector3Test()
    {
        var v1 = new Vector2(4, 4);
        var result = Calculator.RotateVector(v1, Math.PI/2);
        result.X.Should().BeApproximately(-4, 0.001f);
        result.Y.Should().BeApproximately(4, 0.001f);
    }
    
    [Fact]
    public void GetAngle1Test()
    {
        int index = 0;
        var v1 = Converter.ToVector2(_path[index].X, _path[index].Y, _path[index + 1].X, _path[index + 1].Y);
        index++;
        var v2 = Converter.ToVector2(_path[index].X, _path[index].Y, _path[index + 1].X, _path[index + 1].Y);
        
        var result = Calculator.AngleInRadiansLimited(v1, v2);
        result.Should().BeApproximately(Math.PI/2, 0.0001f);
    }
    
    [Fact]
    public void GetAngle2Test()
    {
        int index = 1;
        var v1 = Converter.ToVector2(_path[index].X, _path[index].Y, _path[index + 1].X, _path[index + 1].Y);
        index++;
        var v2 = Converter.ToVector2(_path[index].X, _path[index].Y, _path[index + 1].X, _path[index + 1].Y);

        var result = Calculator.AngleInRadiansLimited(v1, v2);
        result.Should().BeApproximately(Math.PI/2, 0.0001f);
    }
    
    [Fact]
    public void GetAngle3Test()
    {
        int index = 2;
        var v1 = Converter.ToVector2(_path[index].X, _path[index].Y, _path[index + 1].X, _path[index + 1].Y);
        index++;
        var v2 = Converter.ToVector2(_path[index].X, _path[index].Y, _path[index + 1].X, _path[index + 1].Y);

        var result = Calculator.AngleInRadiansLimited(v1, v2);
        result.Should().BeApproximately(-Math.PI/2, 0.0001f);
    }
    
    [Fact]
    public void GetAngle4Test()
    {
        int index = 3;
        var v1 = Converter.ToVector2(_path[index].X, _path[index].Y, _path[index + 1].X, _path[index + 1].Y);
        index++;
        var v2 = Converter.ToVector2(_path[index].X, _path[index].Y, _path[index + 1].X, _path[index + 1].Y);

        var result = Calculator.AngleInRadiansLimited(v1, v2);
        result.Should().BeApproximately(-Math.PI/2, 0.0001f);
    }
    
    [Fact]
    public void GetAngle5Test()
    {

        var v1 = new Vector2(-84.0f, -1.7f);
        var v2 = new Vector2(0.0f, 300.0f);
        var result = Calculator.AngleInRadiansLimited(v1, v2);
        result.Should().BeApproximately(-1.5910316602361894, 0.0001f);// (ca pi/2)
    }
    
    
}