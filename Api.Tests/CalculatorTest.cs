using System.Numerics;
using FluentAssertions;
using SheepHerding.Api.Entities;
using SheepHerding.Api.Helpers;

namespace SheepHerding.Api.Tests;

public class CalculatorTest
{
    private readonly List<Coordinate> _path = new()
    {
        new Coordinate(100, 100), new Coordinate(800, 100), new Coordinate(800, 300), new Coordinate(200, 300),
        new Coordinate(200, 600), new Coordinate(800, 600), new Coordinate(950, 850)
    };

    [Fact]
    public void CentroidTest()
    {
        var result = Calculator.Centroid(new List<Coordinate>
        {
            new(2, 2), new(2, 4), new(4, 2), new(4, 4)
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
        var result = Calculator.RotateVector(v1, Math.PI / 2);
        result.X.Should().BeApproximately(0, 0.001f);
        result.Y.Should().BeApproximately(4, 0.001f);
    }

    [Fact]
    public void RotateVector3Test()
    {
        var v1 = new Vector2(4, 4);
        var result = Calculator.RotateVector(v1, Math.PI / 2);
        result.X.Should().BeApproximately(-4, 0.001f);
        result.Y.Should().BeApproximately(4, 0.001f);
    }

    [Fact]
    public void GetAngle1Test()
    {
        var index = 0;
        var v1 = Converter.ToVector2(_path[index].X, _path[index].Y, _path[index + 1].X, _path[index + 1].Y);
        index++;
        var v2 = Converter.ToVector2(_path[index].X, _path[index].Y, _path[index + 1].X, _path[index + 1].Y);

        var result = Calculator.AngleInRadiansLimited(v1, v2);
        result.Should().BeApproximately(Math.PI / 2, 0.0001f);
    }

    [Fact]
    public void GetAngle2Test()
    {
        var index = 1;
        var v1 = Converter.ToVector2(_path[index].X, _path[index].Y, _path[index + 1].X, _path[index + 1].Y);
        index++;
        var v2 = Converter.ToVector2(_path[index].X, _path[index].Y, _path[index + 1].X, _path[index + 1].Y);

        var result = Calculator.AngleInRadiansLimited(v1, v2);
        result.Should().BeApproximately(Math.PI / 2, 0.0001f);
    }

    [Fact]
    public void GetAngle3Test()
    {
        var index = 2;
        var v1 = Converter.ToVector2(_path[index].X, _path[index].Y, _path[index + 1].X, _path[index + 1].Y);
        index++;
        var v2 = Converter.ToVector2(_path[index].X, _path[index].Y, _path[index + 1].X, _path[index + 1].Y);

        var result = Calculator.AngleInRadiansLimited(v1, v2);
        result.Should().BeApproximately(-Math.PI / 2, 0.0001f);
    }

    [Fact]
    public void GetAngle4Test()
    {
        var index = 3;
        var v1 = Converter.ToVector2(_path[index].X, _path[index].Y, _path[index + 1].X, _path[index + 1].Y);
        index++;
        var v2 = Converter.ToVector2(_path[index].X, _path[index].Y, _path[index + 1].X, _path[index + 1].Y);

        var result = Calculator.AngleInRadiansLimited(v1, v2);
        result.Should().BeApproximately(-Math.PI / 2, 0.0001f);
    }

    [Fact]
    public void GetAngle5Test()
    {
        var v1 = new Vector2(-84.0f, -1.7f);
        var v2 = new Vector2(0.0f, 300.0f);
        var result = Calculator.AngleInRadiansLimited(v1, v2);
        result.Should().BeApproximately(-1.5910316602361894, 0.0001f); // (ca pi/2)
    }

    [Fact]
    public void ExponentialReduceTest()
    {
        // Distance to enemy:   0.0     0.14    0.25    0.66    0.9     1.0
        // Sheep vector lenght: 1.0     0.95    0.91    0.60    0.22    0.0
        var result = Calculator.ExponentialDecrease(0.0, 10.0);
        result.Should().BeApproximately(1.0, 0.0001f);

        result = Calculator.ExponentialDecrease(0.14, 10.0);
        result.Should().BeApproximately(0.957735, 0.0001f);

        result = Calculator.ExponentialDecrease(0.25, 10.0);
        result.Should().BeApproximately(0.9135245, 0.0001f);

        result = Calculator.ExponentialDecrease(0.66, 10.0);
        result.Should().BeApproximately(0.603235, 0.0001f);

        result = Calculator.ExponentialDecrease(0.9, 10.0);
        result.Should().BeApproximately(0.228524, 0.0001f);

        result = Calculator.ExponentialDecrease(1.0, 10.0);
        result.Should().BeApproximately(0.0, 0.0001f);
    }

    [Fact]
    public void OutCastTest()
    {
        //      1   2   3   4   5
        // 1    
        // 2        1       3
        // 3
        // 4        2       4
        // 5                    5
        var coordinates = new List<Coordinate>
        {
            new(2, 2), new(2, 4), new(4, 2), new(4, 4), new(5, 5)
        };
        var center = Calculator.Centroid(coordinates);
        center.X.Should().BeApproximately(3.4, 0.0001);
        center.Y.Should().BeApproximately(3.4, 0.0001);
        var outcast = Calculator.Outcast(coordinates, center);
        outcast.X.Should().Be(5);
        outcast.Y.Should().Be(5);
    }

    [Fact]
    public void NegateLengthWithExponentialDecrease_Test()
    {
        // If a enemy is far away -> less impact on vector
        // If a enemy is close -> exponential impact
        var enemyToCloseStartMoveThreshold = 100.0;
        var maxOutputLenght = 10.0;
        var enemyVector = new Vector2(100, 0);
        var impactVector =
            Calculator.NegateLengthWithExponentialDecrease(enemyVector, enemyToCloseStartMoveThreshold,
                maxOutputLenght);
        impactVector.Length().Should().BeApproximately(0.0f, 0.0001f);
        impactVector.X.Should().BeApproximately(0.0f, 0.0001f);

        // Enemy is closing inn
        enemyVector = new Vector2(75, 0);
        impactVector =
            Calculator.NegateLengthWithExponentialDecrease(enemyVector, enemyToCloseStartMoveThreshold,
                maxOutputLenght);
        impactVector.Length().Should().BeApproximately(4.8628742f, 0.0001f);
        impactVector.X.Should().BeApproximately(-4.862874f, 0.0001f);

        enemyVector = new Vector2(50, 0);
        impactVector =
            Calculator.NegateLengthWithExponentialDecrease(enemyVector, enemyToCloseStartMoveThreshold,
                maxOutputLenght);
        impactVector.Length().Should().BeApproximately(7.597469f, 0.0001f);
        impactVector.X.Should().BeApproximately(-7.5974693f, 0.0001f);

        enemyVector = new Vector2(25, 0);
        impactVector =
            Calculator.NegateLengthWithExponentialDecrease(enemyVector, enemyToCloseStartMoveThreshold,
                maxOutputLenght);
        impactVector.Length().Should().BeApproximately(9.135245f, 0.0001f);
        impactVector.X.Should().BeApproximately(-9.135245f, 0.0001f);

        enemyVector = new Vector2(1, 0);
        impactVector =
            Calculator.NegateLengthWithExponentialDecrease(enemyVector, enemyToCloseStartMoveThreshold,
                maxOutputLenght);
        impactVector.Length().Should().BeApproximately(9.974119f, 0.0001f);
        impactVector.X.Should().BeApproximately(-9.974119f, 0.0001f);
    }
}