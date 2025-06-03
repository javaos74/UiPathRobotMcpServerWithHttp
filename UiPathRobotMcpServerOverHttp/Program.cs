using UiPath.Robot.Api;
using UiPath.Robot.MCP.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;



var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<UiPathRobotTool>();

builder.Services.AddSingleton<RobotClient>(sp =>
{
    var client = new RobotClient();
    return client;
});



var app = builder.Build();

app.MapMcp();

app.Run();
