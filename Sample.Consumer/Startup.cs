using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly:FunctionsStartup(typeof(Sample.Consumer.Startup))]

namespace Sample.Consumer;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var configuration = builder.GetContext().Configuration;

        builder.Services.AddOptions();
        
        builder.Services.Configure<WeatherForecastOptions>(options =>
        {
            options.ServiceBusConnection = configuration["ServiceBusConnection"];
        });
    }
}