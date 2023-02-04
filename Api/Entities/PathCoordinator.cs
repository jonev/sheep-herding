using System.Text;
using SheepHerding.Api.Helpers;

namespace SheepHerding.Api.Entities;

public enum PATH_EXECUTER
{
    SHEEP,
    HERDER
}

public interface IPathMember
{
    public PathCoordinate Get(PATH_EXECUTER executer);
    public PathCoordinate? GetNext(PATH_EXECUTER executer);
}

public class PathCross : IPathMember
{
    public PathCross(PathCoordinate sheeps, PathCoordinate herders)
    {
        Sheeps = sheeps;
        Herders = herders;
    }

    public PathCoordinate Sheeps { get; set; }
    public PathCoordinate Herders { get; set; }
    public IPathMember? Next { get; set; }

    public PathCoordinate Get(PATH_EXECUTER executer)
    {
        if (executer == PATH_EXECUTER.SHEEP) return Sheeps;

        if (executer == PATH_EXECUTER.HERDER) return Herders;

        throw new NotImplementedException("Executer type unknown");
    }

    public PathCoordinate? GetNext(PATH_EXECUTER executer)
    {
        return Next.Get(executer);
    }
}

public class PathCoordinate : IPathMember
{
    public PathCoordinate(Coordinate current, IPathMember? next)
    {
        ThisCoordinate = current;
        Next = next;
    }

    public Coordinate ThisCoordinate { get; set; }
    public IPathMember? Next { get; set; }

    public PathCoordinate Get(PATH_EXECUTER executer)
    {
        return this;
    }

    public PathCoordinate GetNext(PATH_EXECUTER executer)
    {
        if (Next is null) return this;

        return Next?.Get(executer);
    }

    public bool IsEnd()
    {
        return Next is null;
    }

    public bool IsNextCross()
    {
        return Next is PathCross;
    }
}

public class PathCoordinator
{
    private readonly double _intersectionApproachingThreshold;
    private readonly IPathMember _root;
    private PathCoordinate _herd;
    private PathCoordinate _sheep;
    private IList<Coordinate> _startPath;

    public PathCoordinator(double intersectionApproachingThreshold, IPathMember root)
    {
        _intersectionApproachingThreshold = intersectionApproachingThreshold;
        _root = root;
    }

    public void Start()
    {
        if (_root is null) throw new Exception("No root is defined");

        // _startPath = _queue.ToList().Select(c => new Coordinate(c.Left)).ToList();
        _herd = _root.Get(PATH_EXECUTER.HERDER);
        _sheep = _root.Get(PATH_EXECUTER.SHEEP);
    }

    public Coordinate GetCurrent(PATH_EXECUTER executer)
    {
        if (executer == PATH_EXECUTER.SHEEP) return new Coordinate(_sheep.ThisCoordinate);

        if (executer == PATH_EXECUTER.HERDER) return new Coordinate(_herd.ThisCoordinate);

        throw new NotImplementedException("Executer type unknown");
    }

    public Coordinate GetNext(PATH_EXECUTER executer)
    {
        if (executer == PATH_EXECUTER.SHEEP)
        {
            if (_sheep.Next is null) return _sheep.ThisCoordinate;
            return new Coordinate(_sheep.Next.Get(executer).ThisCoordinate);
        }

        if (executer == PATH_EXECUTER.HERDER)
        {
            if (_herd.Next is null) return _herd.ThisCoordinate;
            return new Coordinate(_herd.Next.Get(executer).ThisCoordinate);
        }

        throw new NotImplementedException("Executer type unknown");
    }

    public PathCoordinate? GetNextCross()
    {
        var next = _herd;
        while (!next.IsNextCross())
        {
            next = next.GetNext(PATH_EXECUTER.HERDER);
            if (next.IsEnd()) return null;
        }

        return next.Get(PATH_EXECUTER.HERDER);
    }

    public bool IntersectionApproaching(Coordinate position)
    {
        var cross = GetNextCross();
        if (cross is null) return false;
        var v = Converter.ToVector2(position, cross.ThisCoordinate);
        return v.Length() < _intersectionApproachingThreshold;
    }

    public void Ack(PATH_EXECUTER executer)
    {
        if (_herd.GetNext(PATH_EXECUTER.SHEEP) is null || _sheep.GetNext(PATH_EXECUTER.HERDER) is null) return;

        if (executer == PATH_EXECUTER.SHEEP) _sheep = _sheep.GetNext(PATH_EXECUTER.SHEEP);

        if (executer == PATH_EXECUTER.HERDER) _herd = _herd.GetNext(PATH_EXECUTER.HERDER);
    }

    public void SheepRetreat()
    {
        throw new NotImplementedException();
    }


    public void UpdateToClosest(Coordinate closestTo)
    {
        // TODO Denne tar ikke hensyn til retning av banen. Det gjør at det kan bli usving.
        if (closestTo is null) return;

        if (_herd.ThisCoordinate != null
            || _herd?.GetNext(PATH_EXECUTER.HERDER) != null
            || _herd?.GetNext(PATH_EXECUTER.HERDER)?.ThisCoordinate != null)
            while (Converter.ToVector2(_herd.ThisCoordinate, closestTo).Length()
                   > Converter.ToVector2(_herd?.GetNext(PATH_EXECUTER.HERDER).ThisCoordinate, closestTo).Length())
                Ack(PATH_EXECUTER.HERDER);

        if (_sheep.ThisCoordinate != null
            || _sheep.GetNext(PATH_EXECUTER.SHEEP) != null
            || _sheep.GetNext(PATH_EXECUTER.SHEEP)?.ThisCoordinate != null)
            while (Converter.ToVector2(_sheep.ThisCoordinate, closestTo).Length()
                   > Converter.ToVector2(_sheep.GetNext(PATH_EXECUTER.SHEEP).ThisCoordinate, closestTo).Length())
                Ack(PATH_EXECUTER.SHEEP);
    }

    public string GetStartListAsString()
    {
        var sb = new StringBuilder();
        var current = _herd;
        var currentStart = _herd.ThisCoordinate;
        while (current.Next != null)
        {
            var next = current.GetNext(PATH_EXECUTER.HERDER);
            var currentEnd = next.ThisCoordinate;
            sb.Append($"{currentStart.X},{currentStart.Y},{currentEnd.X},{currentEnd.Y};");
            currentStart = currentEnd;
            current = current.GetNext(PATH_EXECUTER.HERDER);
        }

        current = _sheep;
        currentStart = _sheep.ThisCoordinate;
        while (current.Next != null)
        {
            var next = current.GetNext(PATH_EXECUTER.SHEEP);
            var currentEnd = next.ThisCoordinate;
            sb.Append($"{currentStart.X},{currentStart.Y},{currentEnd.X},{currentEnd.Y};");
            currentStart = currentEnd;
            current = current.GetNext(PATH_EXECUTER.SHEEP);
        }

        var result = sb.ToString();
        return result.Remove(result.Length - 1, 1);
    }

    public List<Coordinate> GetList(PATH_EXECUTER executer)
    {
        var list = new List<Coordinate>();
        if (executer == PATH_EXECUTER.HERDER)
        {
            var current = _herd;
            list.Add(new Coordinate(current.ThisCoordinate));

            while (current.Next is not null)
            {
                if (current.Next is PathCoordinate coordinate)
                    current = coordinate;
                else if (current.Next is PathCross cross) current = cross.Herders;

                list.Add(new Coordinate(current.ThisCoordinate));
            }
        }

        if (executer == PATH_EXECUTER.SHEEP)
        {
            var current = _sheep;
            list.Add(new Coordinate(current.ThisCoordinate));

            while (current.Next is not null)
            {
                if (current.Next is PathCoordinate coordinate)
                    current = coordinate;
                else if (current.Next is PathCross cross) current = cross.Sheeps;

                list.Add(new Coordinate(current.ThisCoordinate));
            }
        }

        return list;
    }
}