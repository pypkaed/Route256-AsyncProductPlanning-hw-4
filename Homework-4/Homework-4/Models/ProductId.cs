using CsvHelper.Configuration.Attributes;
using Homework_4.Exceptions;

namespace Homework_4.Models;

public record ProductId
{
    public ProductId(int id)
    {
        if (id < 0)
        {
            throw ModelException.InvalidModelInput(nameof(ProductId), id);
        }
        
        Id = id;
    }
    
    [Name("id")]
    public long Id { get; private set; }
}