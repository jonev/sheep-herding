using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SheepHerding.Api.Services;

namespace SheepHerding.Api.Tests;

public class HerdServiceTest
{
    [Fact]
    public async Task Test_Path_0()
    {
        var loggerMock = new Mock<ILogger>();
        var service = new HerdService(loggerMock.Object,  null, "testClient");
        try
        {
            service.PathNr = 0;
            service.Connected = true;
            service.Reset = false;
            service.Start = true;
            service.ExecuteAsync();
            await Task.Delay(20000);
            service.Finished.Should().BeTrue();
        }
        finally
        {
            service.Dispose();
        }
    }
}