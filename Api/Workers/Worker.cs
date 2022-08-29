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
        while (!cancellationToken.IsCancellationRequested)
        {
            await _hubContext.Clients.All.SendAsync("Scoreboard", _data.ScoreBoard.OrderBy(s => s.Time), cancellationToken: cancellationToken);
   
            Stopwatch stopwatch = new Stopwatch();   
            var finished = false;
            var listOfSheeps = new List<Sheep>();
            var d = new Drone(200, 200, dt);
            for (int i = 0; i < _data.NrOfSheeps; i++)
            {
                var sheep = new Sheep(200, 200, i, listOfSheeps, d);
                sheep.Set(new Coordinate(100 + ((i%10)* 20 ), 100 + ((i%3) * 20)));
                listOfSheeps.Add(sheep);
            }

            stopwatch.Start();
            while (!cancellationToken.IsCancellationRequested && _data.Reset == false && finished == false)
            {
                
                // Read coorodinates
                d.Set(_data.MousePosition);

                // Calculate centroid of sheeps
                var (x,y) = Calculator.Centroid(listOfSheeps.Select(x => x.Position).ToList());
                
                // Calculate new coordinates
                foreach (var sheep in listOfSheeps)
                {
                    sheep.UpdatePosition(new Coordinate(x, y), dt);
                }

                // Send coordinates
                var print = new List<Coordinate>();
                print.Add(d.Position);
                print.AddRange(listOfSheeps.Select(s => s.Position).ToList());
                var coordinates = CoordinatePrinter.ToString(print);
                
                var vectors = VectorPrinter.ToString(listOfSheeps);
                var message = $"{x},{y}!{coordinates}!{vectors}";
                // _logger.LogDebug($"Sending cooridnates; {message}");
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", "admin", message,
                    cancellationToken: cancellationToken);

                // Finished?
                finished = listOfSheeps.All(s => s.Position.X > 850 && s.Position.Y > 750);
                if (finished)
                {
                    stopwatch.Stop();
                    _data.ScoreBoard.Add(new Score(_data.Name, _data.NrOfSheeps, stopwatch.Elapsed.TotalSeconds));
                    await _hubContext.Clients.All.SendAsync("Scoreboard", _data.ScoreBoard.OrderBy(s => s.Time), cancellationToken: cancellationToken);
                }
                // Wait
                await Task.Delay(dt);
            }

            _data.Reset = false;
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Timed Hosted Service is stopping.");

        return Task.CompletedTask;
    }
}