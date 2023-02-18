using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using SheepHerding.Api.Entities;
using SheepHerding.Api.Services;

namespace SheepHerding.Api.Hubs;

public class VisualizationCommunication : Hub
{
    private static readonly ConcurrentDictionary<string, HerdService> Services = new();

    private readonly IHubContext<VisualizationCommunication> _hubContext;

    // https://docs.microsoft.com/en-us/aspnet/core/tutorials/signalr?WT.mc_id=dotnet-35129-website&view=aspnetcore-6.0&tabs=visual-studio-code
    private readonly ILogger<VisualizationCommunication> _logger;

    public VisualizationCommunication(ILogger<VisualizationCommunication> logger,
        IHubContext<VisualizationCommunication> hubContext)
    {
        _logger = logger;
        _hubContext = hubContext;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"Client connected: {Context.ConnectionId}");
        var service = new HerdService(_logger, _hubContext, Context.ConnectionId, Globals.RandomSeed, new SheepSettings
        {
            RandomAngleRange = Math.PI / 10
        });
        service.ExecuteAsync();
        var result = Services.AddOrUpdate(Context.ConnectionId, service, (_, _) => service);
        _logger.LogInformation($"Client service added: {result.ClientId}");
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"Client disconnected: {Context.ConnectionId} - {exception}");
        var service = Services[Context.ConnectionId];
        service.Dispose();

        return base.OnDisconnectedAsync(exception);
    }

    public void StartStop()
    {
        var service = Services[Context.ConnectionId];
        service.Start = !service.Start;
    }

    public void StartStopDrones()
    {
        var service = Services[Context.ConnectionId];
        service.StartDrones = !service.StartDrones;
    }

    public void ToggleInterceptCross()
    {
        var service = Services[Context.ConnectionId];
        service.InterceptCross = !service.InterceptCross;
    }

    public void Reset(string nr, string s1, string s2, string s3)
    {
        var service = Services[Context.ConnectionId];
        var nrOfSheeps = Convert.ToInt32(nr);
        if (nrOfSheeps < 0) nrOfSheeps = 0;
        if (nrOfSheeps > 200) nrOfSheeps = 200;
        service.NrOfSheeps = nrOfSheeps;
        service.Reset = true;
        service.Start = true;
        service.Failed = false;

        var h = Convert.ToInt32(s1);
        if (h < 0) h = 0;
        if (h > 500) h = 500;
        service.PathNr = h;

        h = Convert.ToInt32(s2);
        if (h < 1) h = 1;
        if (h > 500) h = 500;
        service.RandomAngle = h;

        h = Convert.ToInt32(s3);
        if (h < 0) h = 0;
        if (h > 1000) h = 1000;
        service.VisualizationSpeed = h;
    }

    public async Task SetName(string name)
    {
        var service = Services[Context.ConnectionId];
        service.Name = name;
        await Clients.Client(Context.ConnectionId).SendAsync("SendName", name);
    }

    public void MousePosition(string position)
    {
        var service = Services[Context.ConnectionId];
        var splitted = position.Split(",");
        service.MousePosition.Update(int.Parse(splitted[0]), int.Parse(splitted[1]));
    }

    public new void Dispose()
    {
        _logger.LogInformation("Disposing Hub...");
        var services = Services.Values.ToList();
        foreach (var service in services) service.Dispose();

        base.Dispose();
    }
}