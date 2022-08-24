using System.Collections.ObjectModel;
using Microsoft.AspNetCore.SignalR;
using SignalRDraw.Hubs;
using SignalRDraw.Services;

namespace SignalRDraw.Workers;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHubContext<Communication> _hubContext;
    private readonly PositionService _service;

    public Worker(ILogger<Worker> logger, IHubContext<Communication> hubContext, PositionService service)
    {
        _logger = logger;
        _hubContext = hubContext;
        _service = service;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background Service running.");
        await DoWork(stoppingToken);
    }

    private async Task DoWork(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var listOfSheeps = new List<Sheep>();
            var d = new Drone(200, 200);
            for (int i = 0; i < _service.NrOfSheeps; i++)
            {
                var sheep = new Sheep(400, 400, null, d);
                sheep.Set(new Coordinate(400 + ((i%10)* 20 ), 400 + ((i%3) * 20)));
                listOfSheeps.Add(sheep);
            }


            while (!cancellationToken.IsCancellationRequested && _service.Reset == false)
            {
                
                // Read coorodinates
                d.Set(_service.MousePosition);

                // Calculate new coordinates
                foreach (var sheep in listOfSheeps)
                {
                    sheep.UpdatePosition();
                }

                // Send coordinates
                var print = new List<Coordinate>();
                print.Add(d.Position);
                print.AddRange(listOfSheeps.Select(s => s.Position).ToList());
                var coordinates = CoordinatePrinter.ToString(print);
                
                var vectors = VectorPrinter.ToString(listOfSheeps.Select(s => s.Force).ToList());
                var message = $"{coordinates}!{vectors}";
                _logger.LogDebug($"Sending cooridnates; {message}");
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", "admin", message,
                    cancellationToken: cancellationToken);

                // Wait
                await Task.Delay(10);
            }
            _service.Reset = false;
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Timed Hosted Service is stopping.");

        return Task.CompletedTask;
    }
}