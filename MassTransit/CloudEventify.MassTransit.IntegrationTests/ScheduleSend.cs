using System;
using System.Threading.Tasks;
using FluentAssertions.Extensions;
using Hypothesist;
using MassTransit;
using MassTransit.Serialization.JsonConverters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;
using Xunit.Abstractions;

namespace CloudEventify.MassTransit.IntegrationTests;

[Collection("rabbitmq")]
public class ScheduleSend : IClassFixture<RabbitMqContainer>
{
    private readonly RabbitMqContainer _container;
    private readonly ITestOutputHelper _output;

    public ScheduleSend(RabbitMqContainer container, ITestOutputHelper output)
    {
        _container = container;
        _output = output;
    }

    [Fact]
    public async Task Do()
    {
        var hypothesis = Hypothesis.For<Something>().Any();
        
        LogContext.ConfigureCurrentLogContext(_output.ToLoggerFactory());
        using var host = Host.CreateDefaultBuilder().ConfigureServices(services => 
            services.AddMassTransit(x =>
            {
                x.AddMessageScheduler(new Uri("queue:scheduler"));
                
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.UseInMemoryScheduler("scheduler");
                    cfg.Host(_container.ConnectionString);
                    cfg.UseCloudEvents()
                        .WithJsonOptions(options => options.Converters.Add(new SystemTextJsonConverterFactory()))
                        .WithTypes(t => t
                            .Map<Something>("something"));
                    
                    cfg.ConfigureEndpoints(context);
                });

                x.AddHandler<Something>(c => hypothesis.Test(c.Message));
            })).Build();

        await host.StartAsync();

        var scheduler = host.Services.GetRequiredService<IMessageScheduler>();
        await scheduler.SchedulePublish(DateTime.UtcNow.AddSeconds(2), new Something());
            
        await hypothesis.Validate(5.Seconds());
    }

    public record Something;
}