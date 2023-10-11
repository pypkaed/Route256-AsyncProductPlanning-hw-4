using System.Text.Json;
using Homework_4.Config;

namespace Homework_4;

public class Program
{
    private static string _configFilePath;
    
    public static async Task Main(string[] args)
    {
        _configFilePath = "/home/pypka/RiderProjects/homework-4/Homework-4/Homework-4/Config/config.json";
        
        // TODO: add exception handling and wrong file input
        // TODO: add parser
        // TODO: change it using json
        var parallelOptions = InitializeParallelOptions();
        
        var quit = false;
        while (!quit)
        {
            Console.WriteLine("1. Process file | 2. Change level of parallelism | 3. Quit");
            Int32.TryParse(Console.ReadLine(), out var command);

            try
            {
                switch (command)
                {
                    case 1:
                        await ProcessFile(parallelOptions);
                        break;
                    case 2:
                        ChangeMaxDegreeOfParallelism(parallelOptions);
                        break;
                    case 3:
                        quit = ExitApp();
                        break;
                    default:
                        Console.WriteLine("Wrong input");
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
    
    private static bool ExitApp()
    {
        Console.WriteLine("Exiting app.");
        return true;
    }

    private static string ReadFilePath()
    {
        var filePath = Console.ReadLine();
        if (filePath is null or "")
        {
            throw new Exception("File path is invalid.");
        }

        return filePath;
    }

    private static ParallelOptions InitializeParallelOptions()
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

    private static void ChangeMaxDegreeOfParallelism(ParallelOptions parallelOptions)
    {
        Console.WriteLine("Enter a new value for MaxDegreeOfParallelism: ");
        if (!Int32.TryParse(Console.ReadLine(), out int newMaxDegree))
        {
            Console.WriteLine("MaxDegreeOfParallelism should be an integer.");
            return;
        }

        if (newMaxDegree <= 0)
        {
            Console.WriteLine("MaxDegreeOfParallelism should be positive.");
            return;
        }

        parallelOptions.MaxDegreeOfParallelism = newMaxDegree;

        ChangeFileProcessorConfigMaxParallelism(newMaxDegree);
        
        Console.WriteLine($"MaxDegreeOfParallelism changed to {newMaxDegree}");
    }

    private static void ChangeFileProcessorConfigMaxParallelism(int newMaxDegree)
    {
        var config = new FileProcessorConfig()
        {
            MaxDegreeOfParallelism = newMaxDegree
        };

        string jsonString = JsonSerializer.Serialize<FileProcessorConfig>(config);
        
        File.WriteAllText(_configFilePath, jsonString);
    }

    private static async Task ProcessFile(ParallelOptions parallelOptions)
    {
        Console.WriteLine("Input file path: ");
        var inputFilePath = ReadFilePath();
        Console.WriteLine("Output file path: ");
        var outputFilePath = ReadFilePath();

        var processor = new FileProcessor(_configFilePath);
        await processor.RunProcessing(inputFilePath, outputFilePath, parallelOptions);
    }
}