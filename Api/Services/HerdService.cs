using System.Diagnostics;
using Microsoft.AspNetCore.SignalR;
using SheepHerding.Api.Entities;
using SheepHerding.Api.Helpers;
using SheepHerding.Api.Hubs;

namespace SheepHerding.Api.Services;

public class HerdService : IDisposable
{
    private readonly ILogger _logger;
    private readonly IHubContext<VisualizationCommunication> _hub;
    private readonly int _randomSeed;
    private readonly SheepSettings _sheepSettings;
    public string ClientId { get; }
    private readonly Coordinate Finish = new(870, 770);
    public Coordinate MousePosition { get; set; } = new(0, 0);
    public bool Start { get; set; } = false;
    public bool StartDrones { get; set; } = true;
    public bool Reset { get; set; } = false;
    public int NrOfSheeps { get; set; } = 10;
    public string Name { get; set; } = "Unknown";
    public int VisualizationSpeed { get; set; } = 20;
    public double FailedTimout { get; set; } = 60.0;
    public int PathNr { get; set; } = 6;
    public int RandomAngle { get; set; } = 20;
    public bool Connected { get; set; } = true;
    public bool Finished { get; set; } = false;
    public bool Failed { get; set; } = false;
    public List<Sheep> Sheeps => _listOfSheeps;

    private int _scanTimeDelay = 10;
    private double _scanTime = 0.01;
    private double _forceAdjustment = 0.25;
    private long _previousTicks = DateTime.Now.Ticks;
    private List<Sheep> _listOfSheeps;


    public HerdService(ILogger logger, IHubContext<VisualizationCommunication> hub, string clientId, int randomSeed, SheepSettings sheepSettings)
    {
        _logger = logger;
        _hub = hub;
        _randomSeed = randomSeed;
        _sheepSettings = sheepSettings;
        ClientId = clientId;
        _logger.LogInformation($"Herd service init for: {clientId} --------------------");
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Background Service running.");
        await DoWork();
    }

    private List<DroneHerder> InitializeHerders()
    {
        var listOfHerders = new List<DroneHerder>();
        for (int i = 0; i < 3; i++)
        {
            var h = new DroneHerder(200, 200, i, 8.0);
            h.Set(new Coordinate(100, 100));
            listOfHerders.Add(h);
        }
        return listOfHerders;
    }

    private DroneHerder InitializeMouse()
    {
        var mouse = new DroneHerder(0, 0, -1, 25.0);
        mouse.Set(new Coordinate(1, 1));
        return mouse;
    }

    private void InitializeSheeps(HerdSetup herdSetup, List<DroneHerder> listOfHerders, SheepSettings settings)
    {
        for (int i = 0; i < herdSetup.SheepStartCoordinates.Count; i++)
        {
            var sheep = new Sheep(_logger, i, settings, _listOfSheeps, listOfHerders, Finish, herdSetup.TerrainPath, _randomSeed);
            sheep.Set(herdSetup.SheepStartCoordinates[i]);
            _listOfSheeps.Add(sheep);
        }
    }

    public async Task DoWork()
    {
        while (Connected)
        {
            try
            {
                if(Start) await InitializeAndRun();
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

        _listOfSheeps = new List<Sheep>();

        var listOfHerders = InitializeHerders();
        var mouse = InitializeMouse();
        listOfHerders.Add(mouse);

        var droneOversight = new DroneOversight(_logger, 1000, 500, -1, herdSetup.PredefinedPathCoordinator, listOfHerders,
            new PathCreator(_logger), _listOfSheeps, Finish);
        droneOversight.Set(new Coordinate(150, 100));
        
        InitializeSheeps(herdSetup, listOfHerders, _sheepSettings);

        Stopwatch stopwatch = new Stopwatch();
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
            _sheepSettings.RandomAngleRange = Math.PI/RandomAngle;
            // Mouse
            mouse.UpdatePosition(_forceAdjustment, MousePosition);
            // Calculate new coordinates
            _listOfSheeps.ForEach(sheep => sheep.UpdatePosition(_forceAdjustment));

            var (pathIndex, centroids, current, next, state, oversightPoints, dummy) =
                droneOversight.UpdatePosition(!StartDrones, _forceAdjustment);

            var cast = new List<Point>();
            cast.AddRange(_listOfSheeps);
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
            Finished = _listOfSheeps.All(s => s.IsInsideFinishZone());
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
        _logger.LogInformation($"Exiting: Failed: {Failed}, Finished: {Finished}, Sheeps finished: {_listOfSheeps.Count(s => s.IsInsideFinishZone())}");
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
            $"!{CoordinatePrinter.ToString(_listOfSheeps.Select(s => s.Position).ToList())}" +
            $"!{vectors}" +
            $"!{circle}" +
            $"!{startPath}" +
            $"!{CoordinatePrinter.ToString(herdSetup.PredefinedPathCoordinator.GetList(PATH_EXECUTER.HERDER))}" +
            $"!{CoordinatePrinter.ToString(oversightPoints)}" +
            $"!{state}" +
            $"!{CoordinatePrinter.ToString(herdSetup.TerrainPath.GetList(PATH_EXECUTER.SHEEP))}";
        return message;
    }


    public void Dispose()
    {
        Start = false;
        Connected = false;
        _logger.LogInformation($"Disposing for client: {ClientId}");
    }
}