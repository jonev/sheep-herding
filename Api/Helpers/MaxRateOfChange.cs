namespace SheepHerding.Api.Helpers;

public class MaxRateOfChange
{
    private double _nowValue;

    public MaxRateOfChange(double init = 0.0)
    {
        _nowValue = init;
    }

    public double Limit(double input, double max)
    {
        if (Math.Abs(_nowValue - input) > max)
        {
            if (input > _nowValue)
                _nowValue += max;
            else
                _nowValue -= max;

            return _nowValue;
        }

        return input;
    }
}