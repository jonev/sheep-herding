using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using SheepHerding.Api.Entities;
using SheepHerding.Api.Services;

namespace SheepHerding.Api.Hubs;

public class Communication : Hub
{
    // https://docs.microsoft.com/en-us/aspnet/core/tutorials/signalr?WT.mc_id=dotnet-35129-website&view=aspnetcore-6.0&tabs=visual-studio-code
    private readonly ILogger<Communication> _logger;
    private readonly IHubContext<Communication> _hubContext;
    public static readonly ConcurrentDictionary<string, HerdService> _services = new ();

    public Communication(ILogger<Communication> logger, IHubContext<Communication> hubContext)
    {
        _logger = logger;
        _hubContext = hubContext;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"Client connected: {Context.ConnectionId}");
        var service = new HerdService(_logger, _hubContext, Context.ConnectionId);
        service.ExecuteAsync();
        var result = _services.AddOrUpdate(Context.ConnectionId, service, (key, oldValue) => service);
        _logger.LogInformation($"Client service added: {result.ClientId}");
    }
    
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"Client disconnected: {Context.ConnectionId} - {exception}");
        var service = _services[Context.ConnectionId];
        service.Dispose();

        return base.OnDisconnectedAsync(exception);
    }

    public void StartStop()
    {
        var service = _services[Context.ConnectionId];
        if (service.Start)
        {
            service.Start = false;
        }
        else
        {
            service.Start = true;
        }
    }
    
    public void StartStopDrones()
    {
        var service = _services[Context.ConnectionId];
        if (service.StartDrones)
        {
            service.StartDrones = false;
        }
        else
        {
            service.StartDrones = true;
        }
    }
    
    public void Reset(string nr, string s1, string s2, string s3)
    {
        var service = _services[Context.ConnectionId];
        int nrOfSheeps = Convert.ToInt32(nr);
        if (nrOfSheeps < 0) nrOfSheeps = 0;
        if (nrOfSheeps > 200) nrOfSheeps = 200;
        service.NrOfSheeps = nrOfSheeps;
        service.Reset = true;
        service.Start = true;
        
        int h = Convert.ToInt32(s1);
        if (h < 0) h = 0;
        if (h > 500) h = 500;
        service.PathNr = h;
        //
        // h = Convert.ToInt32(s2);
        // if (h < 1) h = 1;
        // if (h > 500) h = 500;
        // service.HerdAngleInDegrees = h;
        
        h = Convert.ToInt32(s3);
        if (h < 1) h = 1;
        if (h > 1000) h = 10;
        service.VisualizationSpeed = h;
    }
    
    public async Task SetName(string name)
    {
        var service = _services[Context.ConnectionId];
        service.Name = name;
        await Clients.Client(Context.ConnectionId).SendAsync("SendName", name);
    }
    
    public void MousePosition(string position)
    {
        var service = _services[Context.ConnectionId];
        var splitted = position.Split(",");
        service.MousePosition.Update(Int32.Parse(splitted[0]), Int32.Parse(splitted[1]));
    }
}