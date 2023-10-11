using CsvHelper.Configuration.Attributes;

namespace Homework_4.Csv;

public class ProductStatsCsv
{
    public ProductStatsCsv(long id, long prediction, long stock)
    {
        Id = id;
        Prediction = prediction;
        Stock = stock;
    }
    protected ProductStatsCsv() { }
    
    // TODO: VO
    [Name("id")]
    public long Id { get; set; }
    [Name("prediction")]
    public long Prediction { get; set; }
    [Name("stock")]
    public long Stock { get; set; }
}