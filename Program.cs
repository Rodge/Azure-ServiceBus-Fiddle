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