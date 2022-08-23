namespace SignalRDraw.Workers;

public class Sheep : Point
{
    private readonly Drone _enemy;
    internal readonly Vector Force = new ();

    public Sheep(double maxX, double maxY, IList<Sheep> friendlies, Drone enemy) : base(maxX, maxY)
    {
        _enemy = enemy;
    }

    public override void UpdatePosition()
    {
        Heading.Update(Position, _enemy.Position);
        Position.Update(GetPosition());
    }
    
    private Coordinate GetPosition()
    {
        var length = Heading.Lenght > 100 ? 0 : 100 - Heading.Lenght;
        var angle = Heading.Angle + 180;
        Force.Set(Position, angle, length);
        // var x2 = (Position.X + (length * Math.Cos(AngleToRadians(angle))));
        // var y2 = (Position.Y + (length * Math.Sin(AngleToRadians(angle))));
        // return new Coordinate(x2, y2);
        return Force.End;
    }
}

public class Drone : Point
{
    public Drone(double maxX, double maxY) : base(maxX, maxY)
    {
    }

    public override void UpdatePosition()
    {
        throw new NotImplementedException();
    }
}

public abstract class Point
{
    internal readonly Coordinate Position;
    internal readonly Vector Heading;
    private readonly double _maxX;
    private readonly double _maxY;

    public Point(double maxX, double maxY)
    {
        Position = new Coordinate(0, 0);
        Heading = new Vector();
        _maxX = maxX;
        _maxY = maxY;
    }

    public abstract void UpdatePosition();

    public void Set(Coordinate next)
    {
        Position.Update(next);
    }

    public void SetRandomStartPosition()
    {
        var r = new Random();
        Position.Update(r.NextDouble() * _maxX, r.NextDouble() * _maxY);
    }
    
    internal double AngleToRadians(double angle)
    {
        return (Math.PI / 180) * angle;
    }
}