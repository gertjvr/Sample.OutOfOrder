using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.AspNetCore.Mvc;

namespace Sample.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly string _serviceBusConnection;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _serviceBusConnection = configuration["ServiceBusConnection"];
    }

    // [HttpGet(Name = "GetWeatherForecast")]
    // public IEnumerable<WeatherForecast> Get()
    // {
    //     return Enumerable.Range(1, 5).Select(index => new WeatherForecast
    //         {
    //             Date = DateTime.Now.AddDays(index),
    //             TemperatureC = Random.Shared.Next(-20, 55),
    //             Summary = Summaries[Random.Shared.Next(Summaries.Length)]
    //         })
    //         .ToArray();
    // }
    
    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        var queueName = "weather-forecast";
        
        var client = new ServiceBusClient(_serviceBusConnection);
        var admin = new ServiceBusAdministrationClient(_serviceBusConnection);

        var temporaryQueueName = $"{queueName}-{Guid.NewGuid()}";
        await admin.CreateQueueAsync(new CreateQueueOptions(temporaryQueueName)
        {
            AutoDeleteOnIdle = TimeSpan.FromMinutes(6),
        });
        
        var sender = client.CreateSender(queueName);

        var request = JsonSerializer.SerializeToUtf8Bytes(new GetWeatherForecast());

        await sender.SendMessageAsync(new ServiceBusMessage(request)
        {
            ReplyTo = temporaryQueueName
        });
        
        var receiver = client.CreateReceiver(temporaryQueueName);

        var response = await receiver.ReceiveMessageAsync(TimeSpan.FromMilliseconds(3000));

        return JsonSerializer.Deserialize<IEnumerable<WeatherForecast>>(response.Body.ToArray()) ?? Array.Empty<WeatherForecast>();
    }
}

public class GetWeatherForecast
{
}
