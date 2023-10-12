using CsvHelper.Configuration.Attributes;
using Homework_4.Models;

namespace Homework_4.CsvModels;

public class ProductDemandCsv
{
    public ProductDemandCsv(long id, Demand demand)
    {
        Id = id;
        Demand = demand;
    }

    protected ProductDemandCsv() { }

    [Name("id")]
    public long Id { get; set; }
    [Name("demand")]
    public Demand Demand { get; set; }
}