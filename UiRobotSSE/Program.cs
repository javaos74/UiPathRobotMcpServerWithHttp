
using UiPath.Robot.Api;
using UiPathRobotMcpServerOverHttp.Helpers;
using ModelContextProtocol.Protocol;

var tool = new UiPathRobotToolHandler();
//HashSet<string> subscriptions = [];
var builder = WebApplication.CreateBuilder(args);

// Read server settings from configuration
var host = builder.Configuration["ServerSettings:Host"] ?? "0.0.0.0";
var port = builder.Configuration["ServerSettings:Port"] ?? "3002";

// Command line arguments override configuration
// Usage: --host 127.0.0.1 --port 8080
if (args.Length > 0)
{
    for (int i = 0; i < args.Length; i++)
    {
        if (args[i] == "--host" && i + 1 < args.Length)
        {
            host = args[i + 1];
            i++;
        }
        else if (args[i] == "--port" && i + 1 < args.Length)
        {
            port = args[i + 1];
            i++;
        }
    }
}

var serverUrl = $"http://{host}:{port}";
Console.WriteLine($"Starting server on {serverUrl}");

var mcpserverbuider = builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithListToolsHandler(async (request, token) => 
    {
        tool.SetServer(request.Server);
        return await tool.UiPathRobotListHandler(request.Params!, token);
    })
    .WithCallToolHandler(async (request, token) => 
    {
        tool.SetServer(request.Server);
        return await tool.UiPathRobotToolCallHandler(request.Params!, token);
    });

/*
builder.Services.AddSingleton(subscriptions);
builder.Services.AddHostedService<SubscriptionMessageSender>();
*/
var app = builder.Build();

app.MapMcp();

app.UsePathBase("/sse");
app.Run(serverUrl);
