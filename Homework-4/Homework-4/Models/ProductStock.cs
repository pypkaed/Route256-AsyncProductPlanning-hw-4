using CsvHelper.Configuration.Attributes;
using Homework_4.Exceptions;

namespace Homework_4.Models;

public record ProductStock
{
    public ProductStock(long stock)
    {
        if (stock < 0)
        {
            throw ModelException.InvalidModelInput(nameof(ProductStock), stock);
        }

        Stock = stock;
    }
    
    [Name("prediction")]
    public long Stock { get; private set; }
}