using System.Numerics;
using SheepHerding.Api.Entities;

namespace SheepHerding.Api.Helpers;

public static class PathCreator
{
    public static IList<AckableCoordinate> Create90DegreesTurn(Coordinate start, Coordinate end, int startId,
        int nrOfCoordinates)
    {
        var list = new List<AckableCoordinate>();
        double movementInX = (end.X - start.X) / nrOfCoordinates;
        double movementInY = (end.Y - start.Y) / nrOfCoordinates;
        list.Add(new AckableCoordinate(startId, start.X, start.Y));
        for (int i = 1; i < nrOfCoordinates; i++)
        {
            list.Add(new AckableCoordinate(startId + i, start.X + (movementInX * i), start.Y + (movementInY * i)));
        }

        list.Add(new AckableCoordinate(startId + nrOfCoordinates, end.X, end.Y));
        return list;
    }

    public static IList<AckableCoordinate> CurvedLine(Coordinate start, Coordinate end, Coordinate next)
    {
        var nextVector = Converter.ToVector2(end, next);
        var rotatedNormNextVector = Vector2.Normalize(Calculator.RotateVector(nextVector, Math.PI));
        var adjustedNextVector = Vector2.Multiply(rotatedNormNextVector, 50.0f);
        var adjustedEnd = new Coordinate(end.X + adjustedNextVector.X, end.Y + adjustedNextVector.Y);

        var pathLenght = Converter.ToVector2(start.X, start.Y, adjustedEnd.X, adjustedEnd.Y).Length();
        var nrOfPoints = Convert.ToInt32((pathLenght / 25.0));
        var movementInXPerPoint = (adjustedEnd.X - start.X) / nrOfPoints;
        var movementInYPerPoint = (adjustedEnd.Y - start.Y) / nrOfPoints;
        var movementInX = (adjustedEnd.X - start.X);
        var movementInY = (adjustedEnd.Y - start.Y);

        var angle = Math.Atan2(adjustedNextVector.Y, adjustedNextVector.X);

        var oneMovementInRadians = (Math.PI / 2) / (nrOfPoints - 2);


        var list = new List<AckableCoordinate>();
        var startCoordinate = new AckableCoordinate(0, start.X, start.Y);
        startCoordinate.Ack();
        list.Add(startCoordinate);
        for (var i = 1; i < nrOfPoints; i++)
        {
            var cos = Math.Cos((oneMovementInRadians * i) + angle - (Math.PI / 2)) * 100;
            var sin = Math.Sin((oneMovementInRadians * i) + angle - (Math.PI / 2)) * 100;
            list.Add(new AckableCoordinate(i,
                (start.X) + cos + (movementInXPerPoint * (i - 1)) + (movementInX / 4),
                (start.Y + sin + (movementInYPerPoint * (i - 1) + (movementInY / 4)))));
        }

        list.Add(new AckableCoordinate(nrOfPoints, adjustedEnd.X, adjustedEnd.Y));
        return list.OrderBy(c => c.PathIndex).ToList();
    }
}