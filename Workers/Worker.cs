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
        var d = new Drone(200, 200);
        var s1 = new Sheep(200, 200, null, d);
        var s2 = new Sheep(200, 200, null, d);
        var s3 = new Sheep(200, 200, null, d);
        var s4 = new Sheep(200, 200, null, d);
        var s5 = new Sheep(200, 200, null, d);
        s1.Set(new Coordinate(400, 400));
        s2.Set(new Coordinate(450, 400));
        s3.Set(new Coordinate(500, 400));
        s4.Set(new Coordinate(550, 400));
        s5.Set(new Coordinate(600, 400));
        
        
        while (!cancellationToken.IsCancellationRequested)
        {
            // Read coorodinates
            d.Set(_service.MousePosition);

            // Calculate new coordinates
            s1.UpdatePosition();
            s2.UpdatePosition();
            s3.UpdatePosition();
            s4.UpdatePosition();
            s5.UpdatePosition();
            
            // Send coordinates
            var coordinates = CoordinatePrinter.ToString(new List<Coordinate> {d.Position, s1.Position, s2.Position, s3.Position, s4.Position, s5.Position});
            var vectors = VectorPrinter.ToString(new Collection<Vector> {s1.Force, s2.Force, s3.Force, s4.Force, s5.Force});
            var message = $"{coordinates}!{vectors}";
            _logger.LogDebug($"Sending cooridnates; {message}");
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", "admin", message,
                cancellationToken: cancellationToken);

            // Wait
            await Task.Delay(100);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Timed Hosted Service is stopping.");

        return Task.CompletedTask;
    }
}