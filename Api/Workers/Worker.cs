using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using Microsoft.AspNetCore.SignalR;
using SheepHerding.Api.Entities;
using SheepHerding.Api.Helpers;
using SheepHerding.Api.Hubs;
using SheepHerding.Api.Services;

namespace SheepHerding.Api.Workers;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHubContext<Communication> _hubContext;
    private readonly DataSharingService _data;

    public Worker(ILogger<Worker> logger, IHubContext<Communication> hubContext, DataSharingService data)
    {
        _logger = logger;
        _hubContext = hubContext;
        _data = data;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background Service running.");
        await DoWork(stoppingToken);
    }

    private async Task DoWork(CancellationToken cancellationToken)
    {
        var dt = 10;
        var path = new List<Coordinate>
            {new(100, 100), new(800, 100), new(800, 300), new(200, 300), new(200, 600), new(800, 600), new(950, 850)};
        var pathString = CoordinatePrinter.ToString(path);
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("Scoreboard", _data.ScoreBoard.OrderBy(s => s.Points),
                    cancellationToken: cancellationToken);

                Stopwatch stopwatch = new Stopwatch();
                var finished = false;
                var listOfSheeps = new List<Sheep>();
                var listOfHerders = new List<DroneHerder>();
                var droneOversight = new DroneOversight(_logger, 1000, 800, dt);

                var herdSettings = new double[3];
                herdSettings[0] = _data.HerdRadius;
                herdSettings[1] = Calculator.DegreesToRadians(_data.HerdAngleInDegrees);
                herdSettings[2] = _data.OversightSpeed;

                for (int i = 0; i < 3; i++)
                {
                    var h = new DroneHerder(200, 200, i, droneOversight);
                    h.Set(new Coordinate(i * 20, 10));
                    listOfHerders.Add(h);
                }

                for (int i = 0; i < _data.NrOfSheeps; i++)
                {
                    var sheep = new Sheep(200, 200, i, listOfSheeps, listOfHerders, droneOversight);
                    sheep.Set(new Coordinate(400 + ((i % 10) * 20), 100 + ((i % 3) * 20)));
                    listOfSheeps.Add(sheep);
                }

                while (!cancellationToken.IsCancellationRequested && _data.Reset == false && finished == false)
                {
                    if (_data.Start) stopwatch.Start();
                    if (!_data.Start)
                    {
                        stopwatch.Stop();
                        await Task.Delay(1000);
                        continue;
                    }

                    // Calculate centroid of sheeps
                    var (x, y) = Calculator.Centroid(listOfSheeps.Select(x => x.Position).ToList());
                    var largestDistance = listOfSheeps
                        .Select(s => new Vector2((float) (x - s.Position.X), (float) (y - s.Position.Y)).Length())
                        .Max();

                    // Calculate new coordinates
                    foreach (var sheep in listOfSheeps)
                    {
                        sheep.UpdatePosition(new Coordinate(x, y), dt);
                    }

                    var pathIndex = droneOversight.UpdatePosition(new Coordinate(x, y), largestDistance, dt,
                        herdSettings, path);

                    foreach (var herder in listOfHerders)
                    {
                        herder.UpdatePosition(dt);
                    }

                    // Send coordinates
                    var print = new List<Coordinate>();
                    print.Add(droneOversight.Position);
                    print.AddRange(listOfHerders.Select(s => s.Position).ToList());
                    print.AddRange(listOfSheeps.Select(s => s.Position).ToList());
                    var coordinates = CoordinatePrinter.ToString(print);

                    var cast = new List<Point>();
                    cast.AddRange(listOfSheeps);
                    cast.Add(droneOversight);
                    cast.AddRange(listOfHerders);
                    var vectors = VectorPrinter.ToString(cast);
                    var circle = $"{x};{y};{droneOversight.GetHerdingCircleRadius()}";
                    var message =
                        $"{stopwatch.Elapsed.TotalSeconds}!{x},{y}!{coordinates}!{vectors}!{circle}!{pathString}!{CoordinatePrinter.ToString(path.Take(pathIndex + 1).ToList())}";
                    // _logger.LogDebug($"Sending cooridnates; {message}");
                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", "admin", message,
                        cancellationToken: cancellationToken);

                    // Finished?
                    finished = listOfSheeps.All(s => s.Position.X > 850 && s.Position.Y > 750);
                    if (finished)
                    {
                        stopwatch.Stop();
                        AddScore(stopwatch.Elapsed.TotalSeconds);
                        await _hubContext.Clients.All.SendAsync("Scoreboard", _data.ScoreBoard.OrderBy(s => s.Points),
                            cancellationToken: cancellationToken);
                        _data.Start = false;
                    }

                    // Timout
                    if (stopwatch.Elapsed.TotalSeconds > 200.0)
                    {
                        _data.Start = false;
                        _data.Reset = false;
                    }

                    // Wait
                    await Task.Delay(dt);
                }

                _data.Reset = false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Worker loop failed");
                _data.Reset = true;
                _data.Start = false;
            }
        }
    }

    private void AddScore(double timeInSeconds)
    {
        if (_data.ScoreBoard.Count > 999)
        {
            var lowest = _data.ScoreBoard.OrderByDescending(s => s.Points).FirstOrDefault();
            _data.ScoreBoard = new ConcurrentBag<Score>(_data.ScoreBoard.Where(s => s.Time != lowest.Points).ToList());
        }

        _data.ScoreBoard.Add(new Score(_data.Name, _data.NrOfSheeps, timeInSeconds));
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Timed Hosted Service is stopping.");

        return Task.CompletedTask;
    }
}