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
}