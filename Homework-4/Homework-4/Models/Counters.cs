namespace Homework_4.Models;

public class Counters
{
    public Counters()
    {
        ReadLinesCount = 0;
        CalculatedProductDemandsCount = 0;
        WroteEntriesCount = 0;
    }

    public long ReadLinesCount;
    public long CalculatedProductDemandsCount;
    public long WroteEntriesCount;
}