using Microsoft.AspNetCore.SignalR;
using SheepHerding.Api.Services;

namespace SheepHerding.Api.Hubs;

public class Communication : Hub
{
    // https://docs.microsoft.com/en-us/aspnet/core/tutorials/signalr?WT.mc_id=dotnet-35129-website&view=aspnetcore-6.0&tabs=visual-studio-code
    private readonly PositionService _service;

    public Communication(PositionService service)
    {
        _service = service;
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
}