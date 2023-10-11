using System.Globalization;
using System.Threading.Channels;
using CsvHelper;
using CsvHelper.Configuration;
using Homework_4.Csv;
using Homework_4.Models;

namespace Homework_4;

public class Program
{
    public static async Task Main(string[] args)
    {
        // TODO: add exception handling and wrong file input
        // TODO: add parser
        if (args.Length < 1)
        {
            Console.WriteLine("Give me the file path moron...");
            return;
        }
        
        // TODO: add output file option
        // TODO: make into a continuous console
        var filePath = args[0];
        var fileOutputPath = "output.csv";

        var readLinesCount = 0;
        var calculatedProductDemandsCount = 0;
        var wroteEntriesCount = 0;

        // TODO: ???
        using var reader = new StreamReader(filePath);
        await using var writer = new StreamWriter(fileOutputPath);
        using var csvReader = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
        await using var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));
        
        // read from csv yielding to prevent storing everything in memory
        IAsyncEnumerable<ProductStatsCsv> productStats = csvReader.GetRecordsAsync<ProductStatsCsv>();
        
        csvWriter.WriteHeader<ProductDemandCsv>();
        await csvWriter.NextRecordAsync();

        var parallelOptions = new ParallelOptions()
        {
            MaxDegreeOfParallelism = Config.ConfigMaxDegreeOfParallelism
        };
        var channel = Channel.CreateUnbounded<ProductDemandCsv>();

        // run background tasks
        // TODO: turn into long-running tasks?
        _ = Task.Run(() => UserInputListener(parallelOptions));
        _ = Task.Run(() => ConfigChangeListener(parallelOptions));

        var consumeTask = Task.Run(async () =>
        {
            await foreach (var productDemandCsv in channel.Reader.ReadAllAsync())
            {
                if (csvWriter is null) { continue; }
                csvWriter.WriteRecord(productDemandCsv);
                await csvWriter.NextRecordAsync();
            }
        });
        
        await Parallel.ForEachAsync(
            productStats,
            parallelOptions,
            async (productStat, cancellationToken) =>
            {
                // TODO: channels for logging output
                // Console.WriteLine($"Read {Interlocked.Increment(ref readLinesCount)} lines.");

                var demand = new Demand(productStat.Prediction - productStat.Stock);
                var productCsv = new ProductDemandCsv(productStat.Id, demand);
                
                // Console.WriteLine($"Calculated {Interlocked.Increment(ref calculatedProductDemandsCount)} product demands.");
                
                await channel.Writer.WriteAsync(productCsv);
                // Console.WriteLine($"Wrote {Interlocked.Increment(ref wroteEntriesCount)} entries.");
            });
        
        channel.Writer.Complete();
    }

    // TODO: json file with config
    private static void ConfigChangeListener(ParallelOptions parallelOptions)
    {
        
    }
    
    private static void UserInputListener(ParallelOptions parallelOptions)
    {
        while (true)
        {
            Console.WriteLine("Enter a new value for MaxDegreeOfParallelism or 'quit' to exit:");
            string userInput = Console.ReadLine();

            if (userInput == "quit")
            {
                break;
            }

            if (int.TryParse(userInput, out int newMaxDegree))
            {
                parallelOptions.MaxDegreeOfParallelism = newMaxDegree;
                Config.ConfigMaxDegreeOfParallelism = newMaxDegree;
                Console.WriteLine($"MaxDegreeOfParallelism changed to {newMaxDegree}");
            }
            else
            {
                Console.WriteLine("MaxDegreeOfParallelism should be an integer. 'quit' to exit.");
            }
        }
    }
}