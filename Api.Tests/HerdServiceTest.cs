using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SheepHerding.Api.Entities;
using SheepHerding.Api.Services;
using Xunit.Abstractions;

namespace SheepHerding.Api.Tests;

public class HerdServiceTest
{
    private readonly ITestOutputHelper _output;

    public HerdServiceTest(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task Test_Path_SimpleSheeps(int pathNr)
    {
        var loggerMock = new Mock<ILogger>();
        var service = new HerdService(loggerMock.Object,  null, "testClient", Globals.RandomSeed, new SheepSettings
        {
            RandomAngleRange = Math.PI/20.0
        });
        try
        {
            service.PathNr = pathNr;
            service.Connected = true;
            service.Reset = false;
            service.Start = true;
            service.VisualizationSpeed = 0;
            service.FailedTimout = 5.0;
            await service.InitializeAndRun();
            var sheeps = service.Sheeps;
            var nrOfSheepsFinished = sheeps.Where(s => s.IsInsideFinishZone()).Count();
            _output.WriteLine($"Path: {service.PathNr}, Finished: {nrOfSheepsFinished}/{sheeps.Count}");
            service.Finished.Should().BeTrue();
        }
        finally
        {
            service.Dispose();
        }
    }
}