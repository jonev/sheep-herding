using System.Collections.Concurrent;
using SheepHerding.Api.Entities;

namespace SheepHerding.Api.Services;

public class DataSharingService
{
    public ConcurrentBag<Score> ScoreBoard = new ConcurrentBag<Score>();
}
