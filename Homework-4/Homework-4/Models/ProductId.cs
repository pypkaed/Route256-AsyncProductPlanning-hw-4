using CsvHelper.Configuration.Attributes;

namespace Homework_4.Models;

public record ProductId
{
    public ProductId(int id)
    {
        if (id < 0)
        {
            throw new Exception($"Invalid product id: {id}");
        }
        
        Id = id;
    }
    
    [Name("id")]
    public long Id { get; private set; }
}