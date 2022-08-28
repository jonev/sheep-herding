using Microsoft.AspNetCore.SignalR;
using SheepHerding.Api.Services;

namespace SheepHerding.Api.Hubs;

public class Communication : Hub
{
    // https://docs.microsoft.com/en-us/aspnet/core/tutorials/signalr?WT.mc_id=dotnet-35129-website&view=aspnetcore-6.0&tabs=visual-studio-code
    private readonly PositionService _service;
    private readonly ILogger<Communication> _logger;


    public Communication(PositionService service, ILogger<Communication> logger)
    {
        _service = service;
        _logger = logger;
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation($"Client connected: {Context.ConnectionId}");
        return Task.CompletedTask;
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"Client disconnected: {Context.ConnectionId} - {exception}");

        return base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public async Task MousePosition(string position)
    {
       var splitted = position.Split(",");
       _service.MousePosition.Update(Int32.Parse(splitted[0]), Int32.Parse(splitted[1]));
    }
    
    public async Task Reset(string nr)
    {
        _service.NrOfSheeps = Convert.ToInt32(nr);
        _service.Reset = true;
    }
    
    public async Task SetName(string name)
    {
        _service.Name = name;
    }
}