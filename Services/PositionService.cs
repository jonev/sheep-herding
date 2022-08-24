using SignalRDraw.Workers;

namespace SignalRDraw.Services;

public class PositionService
{
    public Coordinate MousePosition { get; set; } = new Coordinate(0, 0);
    public bool Reset { get; set; } = false;
    public int NrOfSheeps { get; set; } = 3;
}
