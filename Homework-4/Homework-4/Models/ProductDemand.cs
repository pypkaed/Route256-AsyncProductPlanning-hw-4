using CsvHelper.Configuration.Attributes;

namespace Homework_4.Models;

public record ProductDemand
{
    public ProductDemand(long demand)
    {
        if (demand < 0)
        {
            demand = 0;
        }

        Demand = demand;
    }
    
    [Name("demand")]
    public long Demand { get; private set; }
}