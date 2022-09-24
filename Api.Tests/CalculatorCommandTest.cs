using FluentAssertions;
using SheepHerding.Api.Entities;
using SheepHerding.Api.Helpers;

namespace SheepHerding.Api.Tests;

public class CalculatorCommandTest
{
    
    [Fact]
    public void GetCommandVectorZero1Test()
    {
        var position = new Coordinate(0, 0);
        var current = new Coordinate(5, 0);
        var next = new Coordinate(10, 0);

        var result = Calculator.GetCommandVector(position, current, next);
        result.X.Should().Be(0);
        result.Y.Should().Be(0);
    }
    
    [Fact]
    public void GetCommandVectorZero2Test()
    {
        var position = new Coordinate(0, 0);
        var current = new Coordinate(5, 0);
        var next = new Coordinate(10, 5);

        var result = Calculator.GetCommandVector(position, current, next);
        result.X.Should().Be(0);
        result.Y.Should().Be(0);
    }
    
    [Fact]
    public void GetCommandVectorZero3Test()
    {
        var position = new Coordinate(0, 0);
        var current = new Coordinate(5, 0);
        var next = new Coordinate(5.01, 5);

        var result = Calculator.GetCommandVector(position, current, next);
        result.X.Should().Be(0);
        result.Y.Should().Be(0);
    }
    
    [Fact]
    public void GetCommandVectorZero4Test()
    {
        var position = new Coordinate(0, 0);
        var current = new Coordinate(5, 0);
        var next = new Coordinate(5, -5);

        var result = Calculator.GetCommandVector(position, current, next);
        result.X.Should().Be(0);
        result.Y.Should().Be(0);
    }
    
    [Fact]
    public void GetCommandVectorZero5Test()
    {
        var position = new Coordinate(5, 0);
        var current = new Coordinate(0, 0);
        var next = new Coordinate(-5, 0);

        var result = Calculator.GetCommandVector(position, current, next);
        result.X.Should().Be(0);
        result.Y.Should().Be(0);
    }
    
    [Fact]
    public void GetCommandVectorZero6Test()
    {
        var position = new Coordinate(5, 0);
        var current = new Coordinate(0, 0);
        var next = new Coordinate(0, -5);

        var result = Calculator.GetCommandVector(position, current, next);
        result.X.Should().Be(0);
        result.Y.Should().Be(0);
    }

    [Fact]
    public void GetCommandVector90DegreesTest()
    {
        var position = new Coordinate(0, 0);
        var current = new Coordinate(5, 0);
        var next = new Coordinate(5, 5);

        var result = Calculator.GetCommandVector(position, current, next);
        result.X.Should().BeApproximately(0, 0.001f);
        result.Y.Should().BeApproximately(-1, 0.001f);
    }
    
    [Fact]
    public void GetCommandVector135DegreesTest()
    {
        var position = new Coordinate(0, 0);
        var current = new Coordinate(5, 0);
        var next = new Coordinate(0, 5);

        var result = Calculator.GetCommandVector(position, current, next);
        result.X.Should().BeApproximately(0, 0.001f);
        result.Y.Should().BeApproximately(-1, 0.001f);
    }
}