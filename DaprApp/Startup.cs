using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DaprApp;

public static class Startup
{
    public static Task Start(params string[] args) => 
        App(Builder(args)).StartAsync();

    public static WebApplication App(Action<WebApplicationBuilder> configure)
    {
        var builder = Builder();
        configure?.Invoke(builder);
        
        return App(builder);
    }

    private static WebApplication App(WebApplicationBuilder builder)
    {
        var app = builder.Build();
        app.UseRouting()
            .UseCloudEvents()
            .UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapSubscribeHandler();
            });

        return app;
    }

    private static WebApplicationBuilder Builder(params string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder
            .Services
            .AddControllers().AddApplicationPart(typeof(Startup).Assembly).AddDapr(options => options.UseGrpcEndpoint("asdf"));
        
        return builder;
    }
}