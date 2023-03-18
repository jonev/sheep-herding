using System.Diagnostics;
using Microsoft.AspNetCore.SignalR;
using SheepHerding.Api.Entities;
using SheepHerding.Api.Helpers;
using SheepHerding.Api.Hubs;

namespace SheepHerding.Api.Services;

public class HerdService : IDisposable
{
    private readonly IHubContext<VisualizationCommunication> _hub;
    private readonly ILogger _logger;
    private readonly int _randomSeed;
    private readonly SheepSettings _sheepSettings;
    private readonly Coordinate Finish = new(870, 770);

    private int _scanTimeDelay = 1;


    public HerdService(ILogger logger, IHubContext<VisualizationCommunication> hub, string clientId, int randomSeed,
        SheepSettings sheepSettings)
    {
        _logger = logger;
        _hub = hub;
        _randomSeed = randomSeed;
        _sheepSettings = sheepSettings;
        ClientId = clientId;
        _logger.LogInformation($"Herd service init for: {clientId} --------------------");
    }

    public string ClientId { get; }
    public Coordinate MousePosition { get; set; } = new(0, 0);
    public bool Start { get; set; }
    public bool StartDrones { get; set; } = true;
    public bool InterceptCross { get; set; } = true;
    public bool Reset { get; set; }
    public int NrOfSheeps { get; set; } = 10;
    public string Name { get; set; } = "Unknown";
    public int VisualizationSpeed { get; set; } = 1;
    public double FailedTimout { get; set; } = 60.0;
    public int PathNr { get; set; } = 10;

    /// <summary>
    ///     Random factor multiplied with PI/100. Min 1, max 100.
    /// </summary>
    public int RandomFactor { get; set; } = 100;

    public bool Connected { get; set; } = true;
    public bool Finished { get; set; }
    public bool Failed { get; set; }
    public List<Sheep> Sheeps { get; private set; }


    public void Dispose()
    {
        Start = false;
        Connected = false;
        _logger.LogInformation($"Disposing for client: {ClientId}");
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Background Service running.");
        await DoWork();
    }

    private List<DroneHerder> InitializeHerders()
    {
        var listOfHerders = new List<DroneHerder>();
        for (var i = 0; i < 3; i++)
        {
            var h = new DroneHerder(200, 200, i, 1.5);
            h.Set(new Coordinate(100, 100));
            listOfHerders.Add(h);
        }

        return listOfHerders;
    }

    private DroneHerder InitializeMouse()
    {
        var mouse = new DroneHerder(0, 0, -1, 3.0);
        mouse.Set(new Coordinate(1, 1));
        return mouse;
    }

    private void InitializeSheeps(HerdSetup herdSetup, List<DroneHerder> listOfHerders, SheepSettings settings)
    {
        for (var i = 0; i < herdSetup.SheepStartCoordinates.Count; i++)
        {
            var sheep = new Sheep(_logger, i, settings, Sheeps, listOfHerders, Finish, herdSetup.TerrainPath,
                _randomSeed);
            sheep.Set(herdSetup.SheepStartCoordinates[i]);
            Sheeps.Add(sheep);
        }
    }

    public async Task DoWork()
    {
        while (Connected)
        {
            try
            {
                if (Start) await InitializeAndRun();
                await Task.Delay(10);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Worker loop failed");
                Reset = true;
                Start = false;
            }

            await Task.Delay(10);
        }

        _logger.LogInformation($"Shutting down {ClientId}");
    }

    public async Task InitializeAndRun()
    {
        // Initializeation --
        var herdSetup = new PredefinedHerdSetup().GetSetup(PathNr);
        herdSetup.PredefinedPathCoordinator.Start();
        var startPath = herdSetup.PredefinedPathCoordinator.GetStartListAsString();
        Finished = false;

        Sheeps = new List<Sheep>();

        var listOfHerders = InitializeHerders();
        var mouse = InitializeMouse();
        listOfHerders.Add(mouse);

        var droneOversight = new DroneOversight(_logger, 1000, 500, -1, herdSetup.PredefinedPathCoordinator,
            listOfHerders,
            new PointCreator(_logger), Sheeps, Finish);
        droneOversight.Set(new Coordinate(150, 100));

        InitializeSheeps(herdSetup, listOfHerders, _sheepSettings);

        var stopwatch = new Stopwatch();
        while (Connected && Reset == false && Finished == false && Failed == false)
        {
            if (Start) stopwatch.Start();
            if (!Start)
            {
                stopwatch.Stop();
                await Task.Delay(1000);
                continue;
            }

            _scanTimeDelay = VisualizationSpeed;
            _sheepSettings.RandomAngleAddedToForce = Math.PI / 100.0 * RandomFactor;
            // Mouse
            mouse.UpdatePosition(MousePosition);
            // Calculate new coordinates
            Sheeps.ForEach(sheep => sheep.UpdatePosition());

            // This is the "Kontrollalgoritme"
            var (pathIndex, centroids, current, next, state, oversightPoints, dummy) =
                droneOversight.UpdatePosition(!StartDrones, InterceptCross);

            var cast = new List<Point>();
            cast.AddRange(Sheeps);
            cast.Add(droneOversight);
            cast.AddRange(listOfHerders);
            cast.Add(dummy);
            // cast.AddRange(oversightPoints);
            if (current != null) oversightPoints.Add(current);
            var vectors = VectorPrinter.ToString(cast);

            var message = CreateMessage(stopwatch, centroids, droneOversight, listOfHerders, vectors,
                herdSetup, startPath, oversightPoints, state);
            // _logger.LogDebug($"Sending cooridnates; {message}");
            if (_hub != null)
                await _hub.Clients.Client(ClientId)
                    .SendAsync("ReceiveMessage", "admin", message);

            // Finished?
            Finished = Sheeps.All(s => s.IsInsideFinishZone());
            if (Finished)
            {
                stopwatch.Stop();
                Start = false;
            }

            // Timout
            if (stopwatch.Elapsed.TotalSeconds > FailedTimout)
            {
                Start = false;
                Reset = false;
                Failed = true;
            }

            // Wait
            await Task.Delay(_scanTimeDelay);
        }

        _logger.LogInformation(
            $"Exiting: Failed: {Failed}, Finished: {Finished}, Sheeps finished: {Sheeps.Count(s => s.IsInsideFinishZone())}");
        Reset = false;
    }

    private string CreateMessage(Stopwatch stopwatch, List<Coordinate> centroids, DroneOversight droneOversight,
        List<DroneHerder> listOfHerders,
        string vectors, HerdSetup herdSetup, string startPath, IList<Coordinate> oversightPoints,
        string state)
    {
        var circle =
            $"{droneOversight.Position.X};{droneOversight.Position.Y};{droneOversight.GetHerdingCircleRadius()}";
        var message =
            $"{stopwatch.Elapsed.TotalSeconds}" +
            $"!{CoordinatePrinter.ToString(centroids)}" +
            $"!{CoordinatePrinter.ToString(new List<Coordinate> {droneOversight.Position})}" +
            $"!{CoordinatePrinter.ToString(listOfHerders.Select(s => s.Position).ToList())}" +
            $"!{CoordinatePrinter.ToString(Sheeps.Select(s => s.Position).ToList())}" +
            $"!{vectors}" +
            $"!{circle}" +
            $"!{startPath}" +
            $"!{CoordinatePrinter.ToString(herdSetup.PredefinedPathCoordinator.GetList(PATH_EXECUTER.HERDER))}" +
            $"!{CoordinatePrinter.ToString(oversightPoints)}" +
            $"!{state}" +
            $"!{CoordinatePrinter.ToString(herdSetup.TerrainPath.GetList(PATH_EXECUTER.SHEEP))}";
        return message;
    }
}