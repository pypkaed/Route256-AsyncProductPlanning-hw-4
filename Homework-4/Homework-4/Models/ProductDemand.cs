using CsvHelper.Configuration.Attributes;

namespace Homework_4.Models;

public record ProductDemand
{
    public ProductDemand(long value)
    {
        if (value < 0)
        {
            value = 0;
        }

        Value = value;
    }
    
    [Name("demand")]
    public long Value { get; private set; }
}