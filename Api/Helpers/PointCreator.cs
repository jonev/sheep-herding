using System.Numerics;
using SheepHerding.Api.Entities;

namespace SheepHerding.Api.Helpers;

public class PointCreator
{
    private readonly ILogger _logger;

    public PointCreator(ILogger logger)
    {
        _logger = logger;
    }

    public IList<AckableCoordinate> CurvedLineToFollowPath(Coordinate start, Coordinate end, Coordinate next)
    {
        var startEndVector = Converter.ToVector2(start, end);
        var endNextVector = Converter.ToVector2(end, next);
        var startEndEndNextAngle = Calculator.AngleInRadiansLimited(startEndVector, endNextVector);


        var onLastPoint = endNextVector.Equals(Vector2.Zero);
        var startEndLenght = startEndVector.Length();

        // Exit if
        if (onLastPoint || Math.Abs(startEndEndNextAngle) < Math.PI / 3 || startEndLenght < 110.0)
            return new List<AckableCoordinate> {new(0, end.X, end.Y)};

        // Set up list with starting point
        var list = new List<AckableCoordinate>();
        var startCoordinate = new AckableCoordinate(0, start.X, start.Y);
        startCoordinate.Ack();
        list.Add(startCoordinate);


        // Settings
        var curveLenght = 80.0f;

        // Set up the line before the curve
        var lineLenght = startEndLenght - curveLenght;
        var nrOfPointsOnLine = (int) (lineLenght / 10);
        var curveStartVector = Vector2.Multiply(Vector2.Normalize(startEndVector), lineLenght);
        var movementInXPerPoint = curveStartVector.X / nrOfPointsOnLine;
        var movementInYPerPoint = curveStartVector.Y / nrOfPointsOnLine;
        for (var i = 1; i <= nrOfPointsOnLine; i++)
            list.Add(new AckableCoordinate(i,
                start.X + movementInXPerPoint * i,
                start.Y + movementInYPerPoint * i));

        // Finding endpoint of curve
        var curveEndVector = Vector2.Multiply(Vector2.Normalize(endNextVector), curveLenght);

        var curvePoints = Bezier(
            new Coordinate(start.X + curveStartVector.X, start.Y + curveStartVector.Y),
            end,
            new Coordinate(end.X + curveEndVector.X, end.Y + curveEndVector.Y),
            nrOfPointsOnLine + 1);
        list.AddRange(curvePoints);

        // _logger?.LogInformation(
        //     $"New path: {nameof(start)}:{start},{nameof(end)}:{end},{nameof(next)}:{next}, {nameof(pathAngle)}:{pathAngle}, {nameof(nextAngle)}:{nextAngle}, {nameof(positionNextAngle)}:{positionNextAngle},  {nameof(startNextAngle)}:{startNextAngle}");
        return list.OrderBy(c => c.PathIndex).ToList();
    }

    // Source: https://towardsdatascience.com/b%C3%A9zier-curve-bfffdadea212
    private double P(double P0, double P1, double P2, double t)
    {
        return Math.Pow(1 - t, 2) * P0 + 2 * t * (1 - t) * P1 + Math.Pow(t, 2) * P2;
    }

    public List<AckableCoordinate> Bezier(Coordinate startCurve, Coordinate zeroPoint, Coordinate endCurve,
        int indexStart)
    {
        var list = new List<AckableCoordinate>();
        for (var t = 0.0; t <= 1.0; t = t + 0.1)
        {
            var x = P(startCurve.X, zeroPoint.X, endCurve.X, t);
            var y = P(startCurve.Y, zeroPoint.Y, endCurve.Y, t);
            list.Add(new AckableCoordinate(indexStart++, x, y, true));
        }

        return list;
    }


    public IList<AckableCoordinate> CurvedLineToFetchHerd(Coordinate start, Coordinate end, Coordinate next)
    {
        var pathVector = Converter.ToVector2(start, end);
        var pathLenght = pathVector.Length();
        var nextVector = Converter.ToVector2(end, next);
        var pathAngle = Math.Atan2(pathVector.Y, pathVector.X);

        var startEndNextAngle =
            Calculator.AngleInRadiansLimited(Converter.ToVector2(end, start), Converter.ToVector2(end, next));
        if (startEndNextAngle < 0) startEndNextAngle += 2 * Math.PI;
        var positionNextAngle = Calculator.AngleInRadiansLimited(pathVector, nextVector);
        if (Math.Abs(positionNextAngle) < Math.PI / 6 || pathLenght < 110.0)
            return new List<AckableCoordinate> {new(0, end.X, end.Y)};

        var angleSignPositive = positionNextAngle > 0 ? true : false;

        var bendRadius = 100.0f; //pathLenght / curveFactor;
        var nrOfPointsInBend = (int) (startEndNextAngle / 2 / (Math.PI / 20));
        var bend = startEndNextAngle / 2 / nrOfPointsInBend;

        var lineLenght = pathLenght - bendRadius;
        var curveCenterVector = Vector2.Multiply(Vector2.Normalize(pathVector), lineLenght);

        var list = new List<AckableCoordinate>();
        var nrOfPointsOnLine = (int) (lineLenght / 10.0);

        var adjustmentRegardsToZeroAngle = angleSignPositive ? -Math.PI / 2 : 0;
        var curveCenter = new Coordinate(start.X + curveCenterVector.X, start.Y + curveCenterVector.Y);

        for (var i = 1; i <= nrOfPointsInBend; i++)
        {
            var cos = Math.Cos(bend * (i - 1) + adjustmentRegardsToZeroAngle + pathAngle) * bendRadius;
            var sin = Math.Sin(bend * (i - 1) + adjustmentRegardsToZeroAngle + pathAngle) * bendRadius;
            list.Add(new AckableCoordinate(
                angleSignPositive ? nrOfPointsOnLine + i : nrOfPointsOnLine + nrOfPointsInBend - i + 1,
                curveCenter.X + cos,
                curveCenter.Y + sin));
        }

        // Adjust according to end
        list = list.OrderBy(c => c.PathIndex).ToList();
        var endOfCircle = list.Last();
        list = list.Select(c =>
            new AckableCoordinate(c.PathIndex, c.X + (end.X - endOfCircle.X), c.Y + (end.Y - endOfCircle.Y))).ToList();

        var endPointOfLine = list.First();
        // var endPoint = list[angleSignPositive ? 1 : nrOfPointsInBend];
        var movementInXPerPoint = (endPointOfLine.X - start.X) / nrOfPointsOnLine;
        var movementInYPerPoint = (endPointOfLine.Y - start.Y) / nrOfPointsOnLine;
        for (var i = 1; i <= nrOfPointsOnLine; i++)
            list.Add(new AckableCoordinate(i,
                start.X + movementInXPerPoint * i,
                start.Y + movementInYPerPoint * i));

        return list.OrderBy(c => c.PathIndex).ToList();
    }
}