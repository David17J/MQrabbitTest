using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MQrabbitTest.Consumer.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<RabbitMqConsumerService>();
    })
    .Build();

await host.RunAsync(); 