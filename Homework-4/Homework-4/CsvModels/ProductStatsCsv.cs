using CsvHelper.Configuration.Attributes;
using Homework_4.Models;

namespace Homework_4.CsvModels;

public class ProductStatsCsv
{
    public ProductStatsCsv(ProductId id, ProductPrediction prediction, ProductStock stock)
    {
        Id = id;
        Prediction = prediction;
        Stock = stock;
    }
    protected ProductStatsCsv() { }
    
    [Name("id")]
    public ProductId Id { get; set; }
    [Name("prediction")]
    public ProductPrediction Prediction { get; set; }
    [Name("stock")]
    public ProductStock Stock { get; set; }
}