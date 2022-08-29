using System.Collections.Concurrent;
using SheepHerding.Api.Entities;

namespace SheepHerding.Api.Services;

public class DataSharingService
{
    public Coordinate MousePosition { get; set; } = new Coordinate(0, 0);
    public bool Start { get; set; } = false;
    public bool Reset { get; set; } = false;
    public int NrOfSheeps { get; set; } = 3;
    public string Name { get; set; } = "Unknown";
    public ConcurrentBag<Score> ScoreBoard = new ConcurrentBag<Score>();
    
    public int HerderPersonalSpaceThreshold { get; set; }
    public int HerderOversightThreshold { get; set; }
    public int HerderOversightSpeed { get; set; }
}
