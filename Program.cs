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
        ServiceBusClient client;

        // the sender used to publish messages to the queue
        ServiceBusSender sender;

        // number of messages to be sent to the queue
        const int numOfMessages = 3;

                // Create the clients that we'll use for sending and processing messages.
        client = new ServiceBusClient(connectionString);
        sender = client.CreateSender(queueName);

        // create a batch 
        using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

        for (int i = 1; i <= 3; i++)
        {
            // try adding a message to the batch
            if (!messageBatch.TryAddMessage(new ServiceBusMessage($"Message {i}")))
            {
                // if an exception occurs
                throw new Exception($"Exception {i} has occurred.");
            }
        }

        try 
        {
            // Use the producer client to send the batch of messages to the Service Bus queue
            await sender.SendMessagesAsync(messageBatch);
            Console.WriteLine($"A batch of {numOfMessages} messages has been published to the queue.");
        }
        finally
        {
            // Calling DisposeAsync on client types is required to ensure that network
            // resources and other unmanaged objects are properly cleaned up.
            await sender.DisposeAsync();
            await client.DisposeAsync();
        }

        Console.WriteLine("Press any key to end the application");
        Console.ReadKey();
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