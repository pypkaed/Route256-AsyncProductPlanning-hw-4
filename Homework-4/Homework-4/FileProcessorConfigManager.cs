using System.Text.Json;
using Homework_4.Config;

namespace Homework_4;

public class FileProcessorConfigManager
{
    private readonly string _configFilePath;
    public FileProcessorConfigManager(string configFilePath)
    {
        _configFilePath = configFilePath;
    }
    
    public ParallelOptions InitializeParallelOptions()
    {
        var jsonString = File.ReadAllText(_configFilePath);
        var config = JsonSerializer.Deserialize<FileProcessorConfig>(jsonString);
        
        var parallelOptions = new ParallelOptions()
        {
            MaxDegreeOfParallelism = config.MaxDegreeOfParallelism
        };

        Console.WriteLine($"Parallel options on initialize: {parallelOptions}");
        return parallelOptions;
    }
    
    public void ChangeFileProcessorConfigMaxParallelism(int newMaxDegree)
    {
        var config = new FileProcessorConfig()
        {
            MaxDegreeOfParallelism = newMaxDegree
        };

        string jsonString = JsonSerializer
            .Serialize<FileProcessorConfig>(config, new JsonSerializerOptions()
            {
                WriteIndented = true
            });

        File.WriteAllText(_configFilePath, jsonString);
    }
}