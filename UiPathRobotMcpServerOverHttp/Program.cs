using Microsoft.Extensions.Configuration.Memory;

using TestServerWithHosting.Tools;
using UiPath.Robot.Api;
using UiPath.Robot.MCP.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;



var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMcpServer()
    .WithHttpTransport()
//    .WithTools<EchoTool>()
//    .WithTools<SampleLlmTool>()
    .WithTools<UiPathRobotTool>();
/*
builder.Services.AddOpenTelemetry()
    .WithTracing(b => b.AddSource("*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation())
    .WithMetrics(b => b.AddMeter("*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation())
    .WithLogging()
    .UseOtlpExporter();
*/

builder.Services.AddSingleton<RobotClient>(sp =>
{
    var client = new RobotClient();
    return client;
});



var app = builder.Build();

app.MapMcp();

app.Run();
