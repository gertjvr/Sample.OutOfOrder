using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Sample.Consumer;

public class GetWeatherForecast
{
}

public class WeatherForecastFunction
{
    private static readonly string[] Summaries = 
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly string _serviceBusConnection;

    public WeatherForecastFunction(IOptions<WeatherForecastOptions> options)
    {
        _serviceBusConnection = options.Value.ServiceBusConnection;
    }
    
    [FunctionName("WeatherForecast")]
    [FixedDelayRetry(5, "00:00:10")]
    public async Task RunAsync([ServiceBusTrigger("weather-forecast", 
            Connection = "ServiceBusConnection")] ServiceBusReceivedMessage received, 
            ILogger log)
    {
        var message = JsonSerializer.Deserialize<GetWeatherForecast>(received.Body.ToStream());
        if (message is null)
            throw new ArgumentException("message is null");

        var weatherForecasts = Enumerable.Range(1, 5)
            .Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        
        await Task.Delay(500);

        if (!string.IsNullOrEmpty(received.ReplyTo))
        {   
            var client = new ServiceBusClient(_serviceBusConnection);
            var sender = client.CreateSender(received.ReplyTo);
            await sender.SendMessageAsync(new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(weatherForecasts)));
        }
    }
}

public class WeatherForecast
{
    public DateTime Date { get; set; }

    public int TemperatureC { get; set; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public string? Summary { get; set; }
}