using CsvHelper.Configuration.Attributes;

namespace Homework_4.Models;

public record ProductPrediction
{
    public ProductPrediction(long value)
    {
        if (value < 0)
        {
            throw new Exception($"Ivalid product prediction {value}");
        }

        Value = value;
    }
    
    [Name("prediction")]
    public long Value { get; private set; }
}