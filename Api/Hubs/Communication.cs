using Microsoft.AspNetCore.SignalR;
using SheepHerding.Api.Services;

namespace SheepHerding.Api.Hubs;

public class Communication : Hub
{
    // https://docs.microsoft.com/en-us/aspnet/core/tutorials/signalr?WT.mc_id=dotnet-35129-website&view=aspnetcore-6.0&tabs=visual-studio-code
    private readonly DataSharingService _data;
    private readonly ILogger<Communication> _logger;


    public Communication(DataSharingService data, ILogger<Communication> logger)
    {
        _data = data;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"Client connected: {Context.ConnectionId}");
        await Clients.All.SendAsync("Scoreboard", _data.ScoreBoard.OrderBy(s => s.Time));
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

    public void MousePosition(string position)
    {
       var splitted = position.Split(",");
       _data.MousePosition.Update(Int32.Parse(splitted[0]), Int32.Parse(splitted[1]));
    }
    
    public void Reset(string nr)
    {
        int nrOfSheeps = Convert.ToInt32(nr);
        if (nrOfSheeps < 3) nrOfSheeps = 3;
        if (nrOfSheeps > 30) nrOfSheeps = 30;
        _data.NrOfSheeps = nrOfSheeps;
        _data.Reset = true;
    }
    
    public void SetName(string name)
    {
        _data.Name = name;
    }
}