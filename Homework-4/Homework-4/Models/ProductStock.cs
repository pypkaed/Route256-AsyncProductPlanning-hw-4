using CsvHelper.Configuration.Attributes;

namespace Homework_4.Models;

public record ProductStock
{
    public ProductStock(long value)
    {
        if (value < 0)
        {
            throw new Exception($"Ivalid product stock {value}");
        }

        Value = value;
    }
    
    [Name("prediction")]
    public long Value { get; private set; }
}