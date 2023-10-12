using System.Text.Json;
using Homework_4.Config;

namespace Homework_4;

public class FileProcessorConfigManager
{
    private readonly string _configFilePath;
    private DateTime _lastModifiedConfigFile;

    public FileProcessorConfigManager(string configFilePath)
    {
        _configFilePath = configFilePath;
        _lastModifiedConfigFile = File.GetLastWriteTime(_configFilePath);
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
    
    public void RunConfigChangeListenerTask(ParallelOptions parallelOptions, CancellationToken cancellationToken)
    {
        // NOTE: a long-running background task to listen to config file changes
        var factory = new TaskFactory();
        var configChangeListenerTask = factory.StartNew(
            () => ConfigChangeListener(parallelOptions),
            TaskCreationOptions.LongRunning);
    }
    
    private async Task ConfigChangeListener(ParallelOptions parallelOptions)
    {
        while (true)
        {
            DateTime modifiedTime = File.GetLastWriteTime(_configFilePath);
            if (_lastModifiedConfigFile.Equals(modifiedTime)) continue;
            
            _lastModifiedConfigFile = modifiedTime;

            var config = DeserealizeConfig();

            parallelOptions.MaxDegreeOfParallelism = config.MaxDegreeOfParallelism;

            Console.WriteLine($"MaxDegreeOfParallelism changed to {config.MaxDegreeOfParallelism}");
        }
    }

    private FileProcessorConfig DeserealizeConfig()
    {
        string jsonString;
        using (var streamReader = new StreamReader(_configFilePath))
        {
            jsonString = streamReader.ReadToEnd();
        }

        var config = JsonSerializer.Deserialize<FileProcessorConfig>(jsonString);
        return config;
    }
}