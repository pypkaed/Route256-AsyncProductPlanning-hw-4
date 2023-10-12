using CsvHelper.Configuration.Attributes;
using Homework_4.Exceptions;

namespace Homework_4.Models;

public record ProductPrediction
{
    public ProductPrediction(long prediction)
    {
        if (prediction < 0)
        {
            throw ModelException.InvalidModelInput(nameof(ProductPrediction), prediction);
        }

        Prediction = prediction;
    }
    
    [Name("prediction")]
    public long Prediction { get; private set; }
}