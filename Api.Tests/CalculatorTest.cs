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
        result.X.Should().BeApproximately(3.0, 0.0001);
        result.Y.Should().BeApproximately(3.0, 0.0001);
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
    
    [Fact]
    public void ExponentialReduceTest()
    {
        var result = Calculator.ExponentialDecrease(0.0, 10.0);
        result.Should().BeApproximately(1.0, 0.0001f);
        
        result = Calculator.ExponentialDecrease(0.14, 10.0);
        result.Should().BeApproximately(0.957735, 0.0001f);
        
        result = Calculator.ExponentialDecrease(0.66, 10.0);
        result.Should().BeApproximately(0.603235, 0.0001f);
        
        result = Calculator.ExponentialDecrease(1.0, 10.0);
        result.Should().BeApproximately(0.0, 0.0001f);
    }
    
    [Fact]
    public void PullTest()
    {
        var max = 10;
        var sheepVcentroid = new Vector2(1, 0);
        var result = Calculator.Pull(sheepVcentroid, sheepVcentroid.Length() / max, 10.0);
        result.X.Should().BeApproximately(0.287f, 0.001f);
        
        sheepVcentroid = new Vector2(2, 0);
        result = Calculator.Pull(sheepVcentroid, sheepVcentroid.Length() / max, 10.0);
        result.X.Should().BeApproximately(0.649f, 0.001f);
        
        sheepVcentroid = new Vector2(3, 0);
        result = Calculator.Pull(sheepVcentroid, sheepVcentroid.Length() / max, 10.0);
        result.X.Should().BeApproximately(1.105f, 0.001f);
        
        sheepVcentroid = new Vector2(4, 0);
        result = Calculator.Pull(sheepVcentroid, sheepVcentroid.Length() / max, 10.0);
        result.X.Should().BeApproximately(1.679f, 0.001f);
        
        sheepVcentroid = new Vector2(5, 0);
        result = Calculator.Pull(sheepVcentroid, sheepVcentroid.Length() / max, 10.0);
        result.X.Should().BeApproximately(2.402f, 0.001f);
        
        sheepVcentroid = new Vector2(6, 0);
        result = Calculator.Pull(sheepVcentroid, sheepVcentroid.Length() / max, 10.0);
        result.X.Should().BeApproximately(3.312f, 0.001f);
        
        sheepVcentroid = new Vector2(7, 0);
        result = Calculator.Pull(sheepVcentroid, sheepVcentroid.Length() / max, 10.0);
        result.X.Should().BeApproximately(4.457f, 0.001f);
        
        sheepVcentroid = new Vector2(8, 0);
        result = Calculator.Pull(sheepVcentroid, sheepVcentroid.Length() / max, 10.0);
        result.X.Should().BeApproximately(5.899f, 0.001f);
        
        sheepVcentroid = new Vector2(9, 0);
        result = Calculator.Pull(sheepVcentroid, sheepVcentroid.Length() / max, 10.0);
        result.X.Should().BeApproximately(7.714f, 0.001f);
        
        sheepVcentroid = new Vector2(10, 0);
        result = Calculator.Pull(sheepVcentroid, sheepVcentroid.Length() / max, 10.0);
        result.X.Should().BeApproximately(10.0f, 0.001f);
    }
    
}