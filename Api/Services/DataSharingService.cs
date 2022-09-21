using System.Collections.Concurrent;
using SheepHerding.Api.Entities;

namespace SheepHerding.Api.Services;

public class DataSharingService
{
    public Coordinate MousePosition { get; set; } = new Coordinate(0, 0);
    public bool Start { get; set; } = false;
    public bool StartDrones { get; set; } = true;
    public bool Reset { get; set; } = false;
    public int NrOfSheeps { get; set; } = 10;
    public string Name { get; set; } = "Unknown";
    public ConcurrentBag<Score> ScoreBoard = new ConcurrentBag<Score>();

    public int HerdRadius { get; set; } = 55;
    public int HerdAngleInDegrees { get; set; } = 85;
    public int OversightSpeed { get; set; } = 5;
}
