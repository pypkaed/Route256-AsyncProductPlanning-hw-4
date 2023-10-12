namespace Homework_4;

public class Program
{
    private static string _configFilePath;
    
    public static async Task Main(string[] args)
    {
        _configFilePath = "/home/pypka/RiderProjects/homework-4/Homework-4/Homework-4/Config/config.json";
        
        // TODO: add exception handling and wrong file input
        var configManager = new FileProcessorConfigManager(_configFilePath);
        var parallelOptions = configManager.InitializeParallelOptions();
        var processor = new FileProcessor();
        
        configManager.RunConfigChangeListenerTask(parallelOptions, new CancellationToken());
        
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
                        await ProcessFile(parallelOptions, processor);
                        break;
                    case 2:
                        ChangeMaxDegreeOfParallelism(parallelOptions, configManager);
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

    private static void ChangeMaxDegreeOfParallelism(ParallelOptions parallelOptions, FileProcessorConfigManager configManager)
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

        configManager.ChangeFileProcessorConfigMaxParallelism(newMaxDegree);
        
        Console.WriteLine($"MaxDegreeOfParallelism changed to {newMaxDegree}");
    }

    private static async Task ProcessFile(ParallelOptions parallelOptions, FileProcessor processor)
    {
        Console.WriteLine("Input file path: ");
        var inputFilePath = ReadFilePath();
        Console.WriteLine("Output file path: ");
        var outputFilePath = ReadFilePath();

        await processor.RunProcessing(inputFilePath, outputFilePath, parallelOptions);
    }
}