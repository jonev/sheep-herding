namespace SheepHerding.Api.Entities;

public class Score
{
    public Score(string name, int nrOfSheeps, double time)
    {
        Name = name;
        NrOfSheeps = nrOfSheeps;
        Time = time;
        Points = time - nrOfSheeps / 4;
    }

    public string Name { get; set; }
    public int NrOfSheeps { get; set; }
    public double Time { get; set; }
    public double Points { get; }
}