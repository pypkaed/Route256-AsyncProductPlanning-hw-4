using System.Globalization;
using System.Text.Json;
using System.Threading.Channels;
using CsvHelper;
using CsvHelper.Configuration;
using Homework_4.Config;
using Homework_4.Csv;
using Homework_4.Models;
using Microsoft.Extensions.Logging;

namespace Homework_4;

public class FileProcessor
{
    private readonly ILogger<FileProcessor> _logger;
    private static string _configFilePath;
    private static DateTime _lastModifiedConfigFile;

    public FileProcessor(string configFilePath)
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddConsole();
        });
        _logger = loggerFactory.CreateLogger<FileProcessor>();

        _configFilePath = configFilePath;
        _lastModifiedConfigFile = File.GetLastWriteTime(_configFilePath);
    }

    public async Task RunProcessing(string inputFilePath, string outputFilePath, ParallelOptions parallelOptions)
    {
        using var reader = new StreamReader(inputFilePath);
        await using var writer = new StreamWriter(outputFilePath);
        using var csvReader = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
        await using var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));
        
        // NOTE: read from csv using IAsyncEnumerable to prevent storing large files in memory
        IAsyncEnumerable<ProductStatsCsv> productStats = csvReader.GetRecordsAsync<ProductStatsCsv>();
        
        csvWriter.WriteHeader<ProductDemandCsv>();
        await csvWriter.NextRecordAsync();
        
        var channel = Channel.CreateUnbounded<ProductDemandCsv>();

        RunConfigChangeListenerTask(parallelOptions, new CancellationToken());
        RunConsumeTask(channel, csvWriter);
        
        await ProcessProductStats(productStats, parallelOptions, channel);
        
        channel.Writer.Complete();
    }

    private async Task ProcessProductStats(
        IAsyncEnumerable<ProductStatsCsv> productStats,
        ParallelOptions parallelOptions,
        Channel<ProductDemandCsv> channel)
    {
        var readLinesCount = 0;
        var calculatedProductDemandsCount = 0;
        var wroteEntriesCount = 0;

        var tasks = new List<Task>();
        
        await foreach (var productStat in productStats)
        {
            if (tasks.Count >= parallelOptions.MaxDegreeOfParallelism)
            {
                await Task.WhenAll(tasks);
                tasks.Clear();
            }
            
            tasks.Add(Task.Run(async () =>
            {
                // TODO: channels for logging output?
                _logger
                    .LogInformation($"Read {Interlocked.Increment(ref readLinesCount)} lines.");
                
                var demand = new Demand(productStat.Prediction - productStat.Stock);
                var productCsv = new ProductDemandCsv(productStat.Id, demand);

                _logger
                    .LogInformation($"Calculated {Interlocked.Increment(ref calculatedProductDemandsCount)} product demands.");
                
                await channel.Writer.WriteAsync(productCsv);
                _logger
                    .LogInformation($"Wrote {Interlocked.Increment(ref wroteEntriesCount)} entries.");
            }));
        }
    }

    private static void RunConsumeTask(Channel<ProductDemandCsv> channel, CsvWriter csvWriter)
    {
        var consumeTask = Task.Run(async () =>
        {
            await foreach (var productDemandCsv in channel.Reader.ReadAllAsync())
            {
                csvWriter.WriteRecord(productDemandCsv);
                await csvWriter.NextRecordAsync();
            }
        });
    }

    private static void RunConfigChangeListenerTask(ParallelOptions parallelOptions, CancellationToken cancellationToken)
    {
        // NOTE: a long-running background task to listen to config file changes
        // var factory = new TaskFactory();
        // var configChangeListenerTask = factory.StartNew(
        //         () => ConfigChangeListener(parallelOptions),
        //         TaskCreationOptions.LongRunning);
        var configChangeListenerTask = Task.Run(() => ConfigChangeListener(parallelOptions));
    }
    
     // TODO: json file with config
    private static async Task ConfigChangeListener(ParallelOptions parallelOptions)
    {
        while (true)
        {
            DateTime modifiedTime = File.GetLastWriteTime(_configFilePath);
            if (_lastModifiedConfigFile.Equals(modifiedTime)) continue;
            
            _lastModifiedConfigFile = modifiedTime;

            string jsonString;
            using (var streamReader = new StreamReader(_configFilePath))
            {
                jsonString = streamReader.ReadToEnd();
            }

            var config = JsonSerializer.Deserialize<FileProcessorConfig>(jsonString);

            parallelOptions.MaxDegreeOfParallelism = config.MaxDegreeOfParallelism;

            Console.WriteLine($"MaxDegreeOfParallelism changed to {config.MaxDegreeOfParallelism}");
        }
    }
}