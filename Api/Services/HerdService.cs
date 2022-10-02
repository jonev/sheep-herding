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
    private readonly DataSharingService _data;
    public string ClientId { get; }
    private readonly Coordinate Finish = new (870, 770);
    public Coordinate MousePosition { get; set; } = new (0, 0);
    public bool Start { get; set; } = false;
    public bool StartDrones { get; set; } = true;
    public bool Reset { get; set; } = false;
    public int NrOfSheeps { get; set; } = 10;
    public string Name { get; set; } = "Unknown";
    public int HerdRadius { get; set; } = 55;
    public int HerdAngleInDegrees { get; set; } = 85;
    public int OversightSpeed { get; set; } = 5;
    public bool Connected { get; set; } = true;
    
    
    public HerdService(ILogger logger, IHubContext<Communication> hub, string clientId, DataSharingService data)
    {
        _logger = logger;
        _hub = hub;
        _data = data;
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
        var dt = 10;
        // var path = new List<AckableCoordinate>
        // {
        //     new(0, 100, 100),
        //     new(1, 200, 150),
        //     new(2, 300, 250),
        //     new(3, 400, 450),
        //     new(4, 600, 500),
        //     new(5, 700, 600),
        //     new(6, 850, 700),
        //     new(7, 950, 850),
        // }; 

        var path = new List<AckableCoordinate>()
        {
            new(0, 150, 200),
            new(1, 300, 200),
            new(2, 400, 200),
            new(3, 500, 200),
            new(4, 600, 200),
            new(5, 800, 200),
            new(6, 800, 400),
            new(7, 400, 400),
            new(8, 200, 400),
            new(9, 200, 700),
            new(10, 900, 900)
        };
        var pathString = CoordinatePrinter.ToString(path.ToList<Coordinate>());
        while (Connected)
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                var finished = false;
                var listOfSheeps = new List<Sheep>();
                var listOfHerders = new List<DroneHerder>();
                var mouse = new DroneHerder(0, 0, -1);

                var herdSettings = new double[3];
                herdSettings[0] = HerdRadius;
                herdSettings[1] = Calculator.DegreesToRadians(HerdAngleInDegrees);
                herdSettings[2] = OversightSpeed;

                for (int i = 0; i < 3; i++)
                {
                    var h = new DroneHerder(200, 200, i);
                    h.Set(new Coordinate(100, 100));
                    listOfHerders.Add(h);
                }

                listOfHerders.Add(mouse);
                var droneOversight = new DroneOversight(_logger, 1000, 800, dt, path, listOfHerders, new PathCreator(_logger), listOfSheeps);
                droneOversight.Set(new Coordinate(150, 100));

                for (int i = 0; i < 2; i++)
                {
                    var sheep = new Sheep(200, 200, i, listOfSheeps, listOfHerders, Finish);
                    sheep.Set(new Coordinate(350 + ((i % 10) * 20), 150 + ((i % 3) * 20)));
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

                while (Connected && Reset == false && finished == false)
                {
                    if (Start) stopwatch.Start();
                    if (!Start)
                    {
                        stopwatch.Stop();
                        await Task.Delay(1000);
                        continue;
                    }

                    // Mouse
                    mouse.UpdatePosition(dt, MousePosition);

                    // Calculate new coordinates
                    foreach (var sheep in listOfSheeps)
                    {
                        sheep.UpdatePosition(dt);
                    }

                    var (pathIndex, centroids, current, next, state, oversightPoints) =
                        droneOversight.UpdatePosition(!StartDrones, dt, herdSettings);
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
                    // cast.AddRange(oversightPoints);
                    if(current != null) oversightPoints.Add(current);
                    var vectors = VectorPrinter.ToString(cast);
                    var circle = $"{droneOversight.Position.X};{droneOversight.Position.Y};{droneOversight.GetHerdingCircleRadius()}";
                    var message =
                        $"{stopwatch.Elapsed.TotalSeconds}!{CoordinatePrinter.ToString(centroids)}!{coordinates}!{vectors}!{circle}!{pathString}!{CoordinatePrinter.ToString(path.ToList<Coordinate>().Take(pathIndex + 1).ToList())}!{CoordinatePrinter.ToString(oversightPoints)}!{state}";
                    // _logger.LogDebug($"Sending cooridnates; {message}");
                    await _hub.Clients.Client(ClientId).SendAsync("ReceiveMessage", "admin", message,
                        default(CancellationToken));

                    // Finished?
                    finished = listOfSheeps.All(s => s.IsInsideFinishZone());
                    if (finished)
                    {
                        stopwatch.Stop();
                        AddScore(stopwatch.Elapsed.TotalSeconds);
                        await _hub.Clients.All.SendAsync("Scoreboard", _data.ScoreBoard.OrderBy(s => s.Points),
                            default);
                        Start = false;
                    }

                    // Timout
                    if (stopwatch.Elapsed.TotalSeconds > 200.0)
                    {
                        Start = false;
                        Reset = false;
                    }

                    // Wait
                    await Task.Delay(dt);
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

    private void AddScore(double timeInSeconds)
    {
        if (_data.ScoreBoard.Count > 999)
        {
            var lowest = _data.ScoreBoard.OrderByDescending(s => s.Points).FirstOrDefault();
            _data.ScoreBoard = new ConcurrentBag<Score>(_data.ScoreBoard.Where(s => s.Time != lowest.Points).ToList());
        }
    
        _data.ScoreBoard.Add(new Score(Name, NrOfSheeps, timeInSeconds));
    }

    public void Dispose()
    {
        Start = false;
        Connected = false;
        _logger.LogInformation($"Disposing for client: {ClientId}");
    }
}