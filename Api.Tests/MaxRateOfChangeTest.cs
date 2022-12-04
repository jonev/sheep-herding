using FluentAssertions;
using SheepHerding.Api.Helpers;

namespace SheepHerding.Api.Tests;

public class MaxRateOfChangeTest
{
    [Fact]
    public void MaxRateOfChangeIncreaseTest()
    {
        var mrc = new MaxRateOfChange();
        var input = 100.0;
        var max = 10.0;

        for (var i = 1; i <= 10; i++)
        {
            var result = mrc.Limit(input, max);
            result.Should().BeApproximately(i * max, 0.01f);
        }
    }

    [Fact]
    public void MaxRateOfChangeDecreaseTest()
    {
        var mrc = new MaxRateOfChange(100.0);
        var input = 0.0;
        var max = 10.0;

        for (var i = 10; i <= 1; i--)
        {
            var result = mrc.Limit(input, max);
            result.Should().BeApproximately(i * max, 0.01f);
        }
    }

    [Fact]
    public void MaxRateOfChangeBothTest()
    {
        var mrc = new MaxRateOfChange();
        var input = 100.0;
        var max = 10.0;

        for (var i = 1; i <= 10; i++)
        {
            var result = mrc.Limit(input, max);
            result.Should().BeApproximately(i * max, 0.01f);
        }

        input = 0.0;
        for (var i = 10; i <= 1; i--)
        {
            var result = mrc.Limit(input, max);
            result.Should().BeApproximately(i * max, 0.01f);
        }
    }
}