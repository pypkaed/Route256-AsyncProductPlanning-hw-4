using System.Globalization;
using System.Threading.Channels;
using CsvHelper;
using CsvHelper.Configuration;
using Homework_4.CsvModels;
using Homework_4.Models;
using Microsoft.Extensions.Logging;

namespace Homework_4;

public class FileProcessor
{
    private readonly ILogger<FileProcessor> _logger;

    public FileProcessor()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddConsole();
        });
        _logger = loggerFactory.CreateLogger<FileProcessor>();
    }

    public async Task RunProcessing(
        string inputFilePath,
        string outputFilePath,
        ParallelOptions parallelOptions,
        CancellationToken cancellationToken)
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

        var consumeTask = RunConsumeTask(channel.Reader, csvWriter, cancellationToken);
        var processTask = ProcessProductStats(productStats, parallelOptions, channel.Writer, cancellationToken);

        await Task.WhenAll(consumeTask, processTask);
        
        channel.Writer.Complete();
    }

    private async Task ProcessProductStats(
        IAsyncEnumerable<ProductStatsCsv> productStats,
        ParallelOptions parallelOptions,
        ChannelWriter<ProductDemandCsv> channelWriter,
        CancellationToken cancellationToken)
    {
        Counters counters = new Counters();

        var tasks = new List<Task>();
        
        await foreach (var productStat in productStats)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (tasks.Count >= parallelOptions.MaxDegreeOfParallelism)
            {
                await Task.WhenAll(tasks);
                tasks.Clear();
            }
            
            tasks.Add(ProcessProductStat(productStat, channelWriter, counters));
        }
    }

    private async Task ProcessProductStat(ProductStatsCsv productStat, ChannelWriter<ProductDemandCsv> channelWriter, Counters counters)
    {
        // TODO: channels for logging output?
        _logger
            .LogInformation($"Read {Interlocked.Increment(ref counters.ReadLinesCount)} lines.");
                
        var demand = new ProductDemand(productStat.Prediction.Prediction - productStat.Stock.Stock);
        var productCsv = new ProductDemandCsv(productStat.Id, demand);

        _logger
            .LogInformation($"Calculated {Interlocked.Increment(ref counters.CalculatedProductDemandsCount)} product demands.");
                
        await channelWriter.WriteAsync(productCsv);
        _logger
            .LogInformation($"Wrote {Interlocked.Increment(ref counters.WroteEntriesCount)} entries.");
    }

    private static Task RunConsumeTask(
        ChannelReader<ProductDemandCsv> channelReader,
        CsvWriter csvWriter,
        CancellationToken cancellationToken)
    {
        var consumeTask = Task.Run(async () =>
        {
            await foreach (var productDemandCsv in channelReader.ReadAllAsync())
            {
                cancellationToken.ThrowIfCancellationRequested();
                csvWriter.WriteRecord(productDemandCsv);
                await csvWriter.NextRecordAsync();
            }
        });

        return consumeTask;
    }
}