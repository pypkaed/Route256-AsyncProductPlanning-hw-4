using CsvHelper.Configuration.Attributes;
using Homework_4.Models;

namespace Homework_4.CsvModels;

public class ProductDemandCsv
{
    public ProductDemandCsv(ProductId id, ProductDemand productDemand)
    {
        Id = id;
        ProductDemand = productDemand;
    }

    protected ProductDemandCsv() { }

    [Name("id")]
    public ProductId Id { get; set; }
    [Name("demand")]
    public ProductDemand ProductDemand { get; set; }
}