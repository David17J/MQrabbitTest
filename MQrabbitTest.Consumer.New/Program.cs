using MQrabbitTest.Consumer.New.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<RabbitMqConsumerService>();
    })
    .Build();

await host.RunAsync();
