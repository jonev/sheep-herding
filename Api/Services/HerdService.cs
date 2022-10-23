using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.AspNetCore.SignalR;
using SheepHerding.Api.Entities;
using SheepHerding.Api.Helpers;
using SheepHerding.Api.Hubs;

namespace SheepHerding.Api.Services;

public class HerdService : IDisposable
{
    private readonly ILogger _logger;
    private readonly IHubContext<Communication> _hub;
    public string ClientId { get; }
    private readonly Coordinate Finish = new(870, 770);
    public Coordinate MousePosition { get; set; } = new(0, 0);
    public bool Start { get; set; } = false;
    public bool StartDrones { get; set; } = true;
    public bool Reset { get; set; } = false;
    public int NrOfSheeps { get; set; } = 10;
    public string Name { get; set; } = "Unknown";
    public int VisualizationSpeed { get; set; } = 20;
    public int PathNr { get; set; } = 0;
    public bool Connected { get; set; } = true;
    public bool Finished { get; set; } = false;

    private int _scanTimeDelay = 10;
    private double _scanTime = 0.01;
    private double _forceAdjustment;
    private long _previousTicks = DateTime.Now.Ticks;


    public HerdService(ILogger logger, IHubContext<Communication> hub, string clientId)
    {
        _logger = logger;
        _hub = hub;
        ClientId = clientId;
        _logger.LogInformation($"Herd service init for: {clientId} --------------------");
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Background Service running.");
        await DoWork();
    }

    private async Task DoWork()
    {
        List<AckableCoordinate> path;
        List<Coordinate> SheepStartCoordinates;
        List<Coordinate> TerrainPathCoordinates;
        while (Connected)
        {
            try
            {
                switch (PathNr)
                {
                    case 0:
                        (path, SheepStartCoordinates, TerrainPathCoordinates) = PredefinedPaths.PathCrossTesting();
                        break;
                    case 1:
                        (path, SheepStartCoordinates, TerrainPathCoordinates) = PredefinedPaths.SmallTurns();
                        break;
                    case 2:
                        (path, SheepStartCoordinates, TerrainPathCoordinates) = PredefinedPaths.TwoUTurns();
                        break;
                    case 3:
                        (path, SheepStartCoordinates, TerrainPathCoordinates) = PredefinedPaths.s90DegreesLeftTestTurn();
                        break;
                    case 4:
                        (path, SheepStartCoordinates, TerrainPathCoordinates) = PredefinedPaths.SmallAnd90DegreesTurn();
                        break;
                    default:
                        (path, SheepStartCoordinates, TerrainPathCoordinates) = PredefinedPaths.SmallAnd90DegreesTurn();
                        break;
                }

                var pathString = CoordinatePrinter.ToString(path.ToList<Coordinate>());
                var terrainPathString = CoordinatePrinter.ToString(TerrainPathCoordinates.ToList<Coordinate>());
                Stopwatch stopwatch = new Stopwatch();
                if(Start) Finished = false;
                var listOfSheeps = new List<Sheep>();
                var listOfHerders = new List<DroneHerder>();
                var mouse = new DroneHerder(0, 0, -1, 25.0);
                mouse.Set(new Coordinate(1, 1));

                for (int i = 0; i < 3; i++)
                {
                    var h = new DroneHerder(200, 200, i, 6.0);
                    h.Set(new Coordinate(100, 100));
                    listOfHerders.Add(h);
                }

                listOfHerders.Add(mouse);
                var droneOversight = new DroneOversight(_logger, 1000, 500, -1, path, listOfHerders,
                    new PathCreator(_logger), listOfSheeps);
                droneOversight.Set(new Coordinate(150, 100));

                _logger.LogInformation($"Number of sheeps: {SheepStartCoordinates.Count}");
                for (int i = 0; i < SheepStartCoordinates.Count; i++)
                {
                    var sheep = new Sheep(_logger, 200, 200, i, listOfSheeps, listOfHerders, Finish, TerrainPathCoordinates);
                    sheep.Set(SheepStartCoordinates[i]);
                    // sheep.Set(new Coordinate(800 + ((i % 10) * 20), 200 + ((i % 3) * 20)));
                    listOfSheeps.Add(sheep);
                }

                // for (int i = NrOfSheeps/2; i < NrOfSheeps; i++)
                // {
                //     var sheep = new Sheep(200, 200, i, listOfSheeps, listOfHerders, Finish);
                //     sheep.Set(new Coordinate(800 + ((i % 10) * 20), 300 + ((i % 3) * 20)));
                //     listOfSheeps.Add(sheep);
                // }

                foreach (var p in path)
                {
                    p.Accessed = false;
                }

                while (Connected && Reset == false && Finished == false)
                {
                    if (Start) stopwatch.Start();
                    if (!Start)
                    {
                        stopwatch.Stop();
                        await Task.Delay(1000);
                        continue;
                    }

                    var ticks = DateTime.Now.Ticks;
                    _scanTime = (ticks - _previousTicks) / 10000000.0;
                    _forceAdjustment = _scanTime * VisualizationSpeed;
                    if (_forceAdjustment > 10) _forceAdjustment = 10.0;
                    _previousTicks = ticks;
                    // _logger.LogInformation($"Scantime: {_scanTime}, {_forceAdjustment}");

                    // Mouse
                    mouse.UpdatePosition(_forceAdjustment, MousePosition);

                    // Calculate new coordinates
                    foreach (var sheep in listOfSheeps)
                    {
                        sheep.UpdatePosition(_forceAdjustment);
                    }

                    var (pathIndex, centroids, current, next, state, oversightPoints) =
                        droneOversight.UpdatePosition(!StartDrones, _forceAdjustment);
                    // var pathIndex = droneOversight.UpdatePosition(new Coordinate(x, y), largestDistance, dt,
                    //     herdSettings, path);

                    var cast = new List<Point>();
                    cast.AddRange(listOfSheeps);
                    cast.Add(droneOversight);
                    cast.AddRange(listOfHerders);
                    // cast.AddRange(oversightPoints);
                    if (current != null) oversightPoints.Add(current);
                    var vectors = VectorPrinter.ToString(cast);
                    var circle =
                        $"{droneOversight.Position.X};{droneOversight.Position.Y};{droneOversight.GetHerdingCircleRadius()}";
                    var message =
                        $"{stopwatch.Elapsed.TotalSeconds}" +
                        $"!{CoordinatePrinter.ToString(centroids)}" +
                        $"!{CoordinatePrinter.ToString(new List<Coordinate>{droneOversight.Position})}" +
                        $"!{CoordinatePrinter.ToString(listOfHerders.Select(s => s.Position).ToList())}" +
                        $"!{CoordinatePrinter.ToString(listOfSheeps.Select(s => s.Position).ToList())}" +
                        $"!{vectors}" +
                        $"!{circle}" +
                        $"!{pathString}" +
                        $"!{CoordinatePrinter.ToString(path.ToList<Coordinate>().Take(pathIndex + 1).ToList())}" +
                        $"!{CoordinatePrinter.ToString(oversightPoints)}" +
                        $"!{state}" +
                        $"!{terrainPathString}";
                    // _logger.LogDebug($"Sending cooridnates; {message}");
                    if(_hub != null) await _hub.Clients.Client(ClientId).SendAsync("ReceiveMessage", "admin", message,
                        default(CancellationToken));

                    // Finished?
                    Finished = listOfSheeps.All(s => s.IsInsideFinishZone());
                    if (Finished)
                    {
                        stopwatch.Stop();
                        Start = false;
                    }

                    // Timout
                    if (stopwatch.Elapsed.TotalSeconds > 200.0)
                    {
                        Start = false;
                        Reset = false;
                    }

                    // Wait
                    await Task.Delay(_scanTimeDelay);
                }

                Reset = false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Worker loop failed");
                Reset = true;
                Start = false;
            }
        }

        _logger.LogInformation($"Shutting down {ClientId}");
    }

    public void Dispose()
    {
        Start = false;
        Connected = false;
        _logger.LogInformation($"Disposing for client: {ClientId}");
    }
}