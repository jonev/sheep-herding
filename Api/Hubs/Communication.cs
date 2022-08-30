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
        await Clients.All.SendAsync("SendName", _data.Name);

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
    
    public void StartStop()
    {
        if (_data.Start)
        {
            _data.Start = false;
        }
        else
        {
            _data.Start = true;
        }
    }
    
    public void Reset(string nr, string s1, string s2, string s3)
    {
        int nrOfSheeps = Convert.ToInt32(nr);
        if (nrOfSheeps < 100) nrOfSheeps = 100;
        if (nrOfSheeps > 200) nrOfSheeps = 200;
        _data.NrOfSheeps = nrOfSheeps;
        _data.Reset = true;
        
        int h = Convert.ToInt32(s1);
        if (h < 0) h = 0;
        if (h > 500) h = 500;
        _data.HerdRadius = h;
        
        h = Convert.ToInt32(s2);
        if (h < 0) h = 0;
        if (h > 500) h = 500;
        _data.HerdAngleInDegrees = h;
        
        h = Convert.ToInt32(s3);
        if (h < 0) h = 0;
        if (h > 500) h = 500;
        _data.OversightSpeed = h;
    }
    
    public async Task SetName(string name)
    {
        _data.Name = name;
        await Clients.All.SendAsync("SendName", name);
    }
}