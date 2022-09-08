using System.Threading.Tasks;  
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

public class Program 
{
    public static async Task Main(string[] args)
    {
        // Build a config object, using env vars and JSON providers.
        var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environmentName}.json", true, true)
            .AddEnvironmentVariables()
            .Build();

        // Get values from the config given their key and their target type.
        ConnectionStrings connectionStrings = config.GetRequiredSection("ConnectionStrings").Get<ConnectionStrings>();

        // Write the values to the console.
        Console.WriteLine($"connectionStrings.ServiceBus.PrimaryConnectionString = {connectionStrings.ServiceBus.PrimaryConnectionString}\n");

        // connection string to your Service Bus namespace
        string connectionString = connectionStrings.ServiceBus.PrimaryConnectionString;

        // name of your Service Bus topic
        string queueName = "az204-queue";

        // the client that owns the connection and can be used to create senders and receivers
        ServiceBusClient client = new ServiceBusClient(connectionString);
        
        // create a processor that we can use to process the messages
        var processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

        try
        {
            // add handler to process messages
            processor.ProcessMessageAsync += MessageHandler;

            // add handler to process any errors
            processor.ProcessErrorAsync += ErrorHandler;

            // start processing 
            await processor.StartProcessingAsync();

            Console.WriteLine("Wait for a minute and then press any key to end the processing");
            Console.ReadKey();

            // stop processing 
            Console.WriteLine("\nStopping the receiver...");
            await processor.StopProcessingAsync();
            Console.WriteLine("Stopped receiving messages");
        }
        finally
        {
            // Calling DisposeAsync on client types is required to ensure that network
            // resources and other unmanaged objects are properly cleaned up.
            await processor.DisposeAsync();
            await client.DisposeAsync();
        }
    }

    // handle received messages
    static async Task MessageHandler(ProcessMessageEventArgs args)
    {
        string body = args.Message.Body.ToString();
        Console.WriteLine($"Received: {body}");

        // complete the message. messages is deleted from the queue. 
        await args.CompleteMessageAsync(args.Message);
    }

    // handle any errors when receiving messages
    static Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }
}

public sealed class ConnectionStrings
{
    public ServiceBus ServiceBus { get; set; } = null!;
}

public sealed class ServiceBus
{
    public string PrimaryConnectionString { get; set; } = null!;
}