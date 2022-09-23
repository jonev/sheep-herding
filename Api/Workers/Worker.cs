using System.Collections.Concurrent;
using System.Diagnostics;
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
    private readonly Coordinate Finish = new Coordinate(870, 770);

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
        var path = new List<AckableCoordinate>
        {
            new(0, 100, 100),
            new(1, 200, 150),
            new(2, 300, 250),
            new(3, 400, 450),
            new(4, 600, 500),
            new(5, 700, 600),
            new(6, 850, 700),
            new(7, 950, 850),
        }; //, new(800, 300), new(200, 300), new(200, 600), new(800, 600), new(950, 850)};
        var pathString = CoordinatePrinter.ToString(path.ToList<Coordinate>());
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
                var mouse = new DroneHerder(0, 0, -1);

                var herdSettings = new double[3];
                herdSettings[0] = _data.HerdRadius;
                herdSettings[1] = Calculator.DegreesToRadians(_data.HerdAngleInDegrees);
                herdSettings[2] = _data.OversightSpeed;

                for (int i = 0; i < 3; i++)
                {
                    var h = new DroneHerder(200, 200, i);
                    h.Set(new Coordinate(100, 100));
                    listOfHerders.Add(h);
                }

                listOfHerders.Add(mouse);
                var droneOversight = new DroneOversight(_logger, 1000, 800, dt, path, listOfHerders);
                droneOversight.Set(new Coordinate(100, 100));

                for (int i = 0; i < _data.NrOfSheeps/2; i++)
                {
                    var sheep = new Sheep(200, 200, i, listOfSheeps, listOfHerders, Finish);
                    sheep.Set(new Coordinate(400 + ((i % 10) * 20), 200 + ((i % 3) * 20)));
                    listOfSheeps.Add(sheep);
                }

                // for (int i = _data.NrOfSheeps/2; i < _data.NrOfSheeps; i++)
                // {
                //     var sheep = new Sheep(200, 200, i, listOfSheeps, listOfHerders, Finish);
                //     sheep.Set(new Coordinate(800 + ((i % 10) * 20), 300 + ((i % 3) * 20)));
                //     listOfSheeps.Add(sheep);
                // }

                foreach (var p in path)
                {
                    p.Accessed = false;
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
                    // var (x, y) = Calculator.Centroid(listOfSheeps.Select(x => x.Position).ToList());
                    // var largestDistance = listOfSheeps
                    // .Select(s => new Vector2((float) (x - s.Position.X), (float) (y - s.Position.Y)).Length())
                    // .Max();

                    // Mouse
                    mouse.UpdatePosition(dt, _data.MousePosition);

                    // Calculate new coordinates
                    foreach (var sheep in listOfSheeps)
                    {
                        sheep.UpdatePosition(dt);
                    }

                    var (pathIndex, centroids, current, next, state, oversightPoints) =
                        droneOversight.UpdatePosition(!_data.StartDrones, dt, herdSettings, listOfSheeps);
                    // var pathIndex = droneOversight.UpdatePosition(new Coordinate(x, y), largestDistance, dt,
                    //     herdSettings, path);

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
                    cast.AddRange(oversightPoints);
                    var vectors = VectorPrinter.ToString(cast);
                    var circle = $"{droneOversight.Position.X};{droneOversight.Position.Y};{droneOversight.GetHerdingCircleRadius()}";
                    var message =
                        $"{stopwatch.Elapsed.TotalSeconds}!{CoordinatePrinter.ToString(centroids)}!{coordinates}!{vectors}!{circle}!{pathString}!{CoordinatePrinter.ToString(path.ToList<Coordinate>().Take(pathIndex + 1).ToList())}!{CoordinatePrinter.ToString(new List<Coordinate> {current, next})}!{state}";
                    // _logger.LogDebug($"Sending cooridnates; {message}");
                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", "admin", message,
                        cancellationToken: cancellationToken);

                    // Finished?
                    finished = listOfSheeps.All(s => s.IsInsideFinishZone());
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