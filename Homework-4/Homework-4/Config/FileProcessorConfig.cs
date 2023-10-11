using System.Text.Json.Serialization;

namespace Homework_4.Config;

public record FileProcessorConfig
{
    public FileProcessorConfig() { }

    public FileProcessorConfig(int maxDegreeOfParallelism)
    {
        if (maxDegreeOfParallelism <= 0)
        {
            throw new Exception("Nah.");
        }
        MaxDegreeOfParallelism = maxDegreeOfParallelism;
    }
    
    [JsonPropertyName("maxDegreeOfParallelism")]
    public int MaxDegreeOfParallelism { get; set; }
}