using FluentAssertions;
using SheepHerding.Api.Entities;

namespace SheepHerding.Api.Tests;

public class PathCoordinatesTest
{
    [Fact]
    public void Test_Add_Coordinates_Simple_One_Line()
    {
        var p = new PathCoordinator
        (
            new PathCoordinate(new Coordinate(0,0), 
                new PathCoordinate(new Coordinate(1,0), 
                    new PathCoordinate(new Coordinate(2,0),
                        new PathCoordinate(new Coordinate(3,0), null)
        ))));
        p.Start();
        p.GetCurrent(PATH_EXECUTER.HERDER).X.Should().Be(0);
        p.GetCurrent(PATH_EXECUTER.SHEEP).X.Should().Be(0);
        p.Ack(PATH_EXECUTER.HERDER);
        p.Ack(PATH_EXECUTER.SHEEP);
        p.GetCurrent(PATH_EXECUTER.HERDER).X.Should().Be(1);
        p.GetCurrent(PATH_EXECUTER.SHEEP).X.Should().Be(1);
        p.Ack(PATH_EXECUTER.HERDER);
        p.Ack(PATH_EXECUTER.SHEEP);
        p.GetCurrent(PATH_EXECUTER.HERDER).X.Should().Be(2);
        p.GetCurrent(PATH_EXECUTER.SHEEP).X.Should().Be(2);
        p.Ack(PATH_EXECUTER.HERDER);
        p.Ack(PATH_EXECUTER.SHEEP);
        p.GetCurrent(PATH_EXECUTER.HERDER).X.Should().Be(3);
        p.GetCurrent(PATH_EXECUTER.SHEEP).X.Should().Be(3);
        p.Ack(PATH_EXECUTER.HERDER);
        p.Ack(PATH_EXECUTER.SHEEP);
        p.GetCurrent(PATH_EXECUTER.HERDER).X.Should().Be(3);
        p.GetCurrent(PATH_EXECUTER.SHEEP).X.Should().Be(3);
    }
    
    [Fact]
    public void Test_Add_Coordinates_Simple_One_Cross()
    {
        var p = new PathCoordinator
        (
            new PathCoordinate(new Coordinate(0,0), 
                new PathCoordinate(new Coordinate(1,0), 
                    new PathCoordinate(new Coordinate(2,0),
                        new PathCross(
                            new PathCoordinate(new Coordinate(3,0), 
                                new PathCoordinate(new Coordinate(4,0),
                                    new PathCoordinate(new Coordinate(5,0),
                                        new PathCoordinate(new Coordinate(6,0), null)))),
                            new PathCoordinate(new Coordinate(33,0), 
                                new PathCoordinate(new Coordinate(44,0),
                                    new PathCoordinate(new Coordinate(55,0),
                                        new PathCoordinate(new Coordinate(66,0), null))))
                            )))));
        p.Start();
        p.GetCurrent(PATH_EXECUTER.HERDER).X.Should().Be(0);
        p.GetCurrent(PATH_EXECUTER.SHEEP).X.Should().Be(0);
        p.Ack(PATH_EXECUTER.HERDER);
        p.Ack(PATH_EXECUTER.SHEEP);
        p.GetCurrent(PATH_EXECUTER.HERDER).X.Should().Be(1);
        p.GetCurrent(PATH_EXECUTER.SHEEP).X.Should().Be(1);
        p.Ack(PATH_EXECUTER.HERDER);
        p.Ack(PATH_EXECUTER.SHEEP);
        p.GetCurrent(PATH_EXECUTER.HERDER).X.Should().Be(2);
        p.GetCurrent(PATH_EXECUTER.SHEEP).X.Should().Be(2);
        // Cross
        p.Ack(PATH_EXECUTER.HERDER);
        p.Ack(PATH_EXECUTER.SHEEP);
        p.GetCurrent(PATH_EXECUTER.HERDER).X.Should().Be(33);
        p.GetCurrent(PATH_EXECUTER.SHEEP).X.Should().Be(3);
        p.Ack(PATH_EXECUTER.HERDER);
        p.Ack(PATH_EXECUTER.SHEEP);
        p.GetCurrent(PATH_EXECUTER.HERDER).X.Should().Be(44);
        p.GetCurrent(PATH_EXECUTER.SHEEP).X.Should().Be(4);
        p.Ack(PATH_EXECUTER.HERDER);
        p.Ack(PATH_EXECUTER.SHEEP);
        p.GetCurrent(PATH_EXECUTER.HERDER).X.Should().Be(55);
        p.GetCurrent(PATH_EXECUTER.SHEEP).X.Should().Be(5);
        p.Ack(PATH_EXECUTER.HERDER);
        p.Ack(PATH_EXECUTER.SHEEP);
        p.GetCurrent(PATH_EXECUTER.HERDER).X.Should().Be(66);
        p.GetCurrent(PATH_EXECUTER.SHEEP).X.Should().Be(6);
    }
    
    [Fact]
    public void Test_Add_Coordinates_Simple_One_Cross_SheepRetreat()
    {
        var p = new PathCoordinator
        (
            new PathCoordinate(new Coordinate(0,0), 
                new PathCoordinate(new Coordinate(1,0), 
                    new PathCoordinate(new Coordinate(2,0),
                        new PathCross(
                            new PathCoordinate(new Coordinate(3,0), 
                                new PathCoordinate(new Coordinate(4,0),
                                    new PathCoordinate(new Coordinate(5,0),
                                        new PathCoordinate(new Coordinate(6,0), null)))),
                            new PathCoordinate(new Coordinate(33,0), 
                                new PathCoordinate(new Coordinate(44,0),
                                    new PathCoordinate(new Coordinate(55,0),
                                        new PathCoordinate(new Coordinate(66,0), null))))
                        )))));
        p.Start();
        p.GetCurrent(PATH_EXECUTER.HERDER).X.Should().Be(0);
        p.GetCurrent(PATH_EXECUTER.SHEEP).X.Should().Be(0);
        p.Ack(PATH_EXECUTER.HERDER);
        p.Ack(PATH_EXECUTER.SHEEP);
        p.GetCurrent(PATH_EXECUTER.HERDER).X.Should().Be(1);
        p.GetCurrent(PATH_EXECUTER.SHEEP).X.Should().Be(1);
        p.Ack(PATH_EXECUTER.HERDER);
        p.Ack(PATH_EXECUTER.SHEEP);
        p.GetCurrent(PATH_EXECUTER.HERDER).X.Should().Be(2);
        p.GetCurrent(PATH_EXECUTER.SHEEP).X.Should().Be(2);
        // Cross
        p.Ack(PATH_EXECUTER.HERDER);
        p.Ack(PATH_EXECUTER.SHEEP);
        p.GetCurrent(PATH_EXECUTER.HERDER).X.Should().Be(33);
        p.GetCurrent(PATH_EXECUTER.SHEEP).X.Should().Be(3);
        p.Ack(PATH_EXECUTER.HERDER);
        p.Ack(PATH_EXECUTER.SHEEP);
        p.GetCurrent(PATH_EXECUTER.HERDER).X.Should().Be(44);
        p.GetCurrent(PATH_EXECUTER.SHEEP).X.Should().Be(4);
        p.Ack(PATH_EXECUTER.HERDER);
        p.Ack(PATH_EXECUTER.SHEEP);
        p.GetCurrent(PATH_EXECUTER.HERDER).X.Should().Be(55);
        p.GetCurrent(PATH_EXECUTER.SHEEP).X.Should().Be(5);
        p.Ack(PATH_EXECUTER.HERDER);
        p.Ack(PATH_EXECUTER.SHEEP);
        p.GetCurrent(PATH_EXECUTER.HERDER).X.Should().Be(66);
        p.GetCurrent(PATH_EXECUTER.SHEEP).X.Should().Be(6);
    }
    
    [Fact]
    public void Test_ToList()
    {
        var p = new PathCoordinator
        (
            new PathCoordinate(new Coordinate(0,0), 
                new PathCoordinate(new Coordinate(1,0), 
                    new PathCoordinate(new Coordinate(2,0),
                        new PathCross(
                            new PathCoordinate(new Coordinate(3,0), 
                                new PathCoordinate(new Coordinate(4,0),
                                    new PathCoordinate(new Coordinate(5,0),
                                        new PathCoordinate(new Coordinate(6,0), null)))),
                            new PathCoordinate(new Coordinate(33,0), 
                                new PathCoordinate(new Coordinate(44,0),
                                    new PathCoordinate(new Coordinate(55,0),
                                        new PathCoordinate(new Coordinate(66,0), null))))
                        )))));
        p.Start();
        var sheepList = p.GetList(PATH_EXECUTER.SHEEP);
        sheepList[0].X.Should().Be(0);
        sheepList[2].X.Should().Be(2);
        sheepList[3].X.Should().Be(3);
        sheepList[4].X.Should().Be(4);
        sheepList[6].X.Should().Be(6);
        var herdList = p.GetList(PATH_EXECUTER.HERDER);
        herdList[0].X.Should().Be(0);
        herdList[2].X.Should().Be(2);
        herdList[3].X.Should().Be(33);
        herdList[4].X.Should().Be(44);
        herdList[6].X.Should().Be(66);
    }

    [Fact]
    public void Test_PathCoordinatorPrinter()
    {
        var p = new PathCoordinator
        (
            new PathCoordinate(new Coordinate(0,0), 
                new PathCoordinate(new Coordinate(1,0), 
                    new PathCoordinate(new Coordinate(2,0),
                        new PathCross(
                            new PathCoordinate(new Coordinate(3,0), 
                                new PathCoordinate(new Coordinate(4,0),
                                    new PathCoordinate(new Coordinate(5,0),
                                        new PathCoordinate(new Coordinate(6,0), null)))),
                            new PathCoordinate(new Coordinate(33,0), 
                                new PathCoordinate(new Coordinate(44,0),
                                    new PathCoordinate(new Coordinate(55,0),
                                        new PathCoordinate(new Coordinate(66,0), null))))
                        )))));
        p.Start();
        var result = p.GetStartListAsString();
        result.Should().Be("0,0,1,0;1,0,2,0;2,0,33,0;33,0,44,0;44,0,55,0;55,0,66,0;0,0,1,0;1,0,2,0;2,0,3,0;3,0,4,0;4,0,5,0;5,0,6,0");
    }
}