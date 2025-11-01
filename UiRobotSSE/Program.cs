
using UiPath.Robot.Api;
using UiPathRobotMcpServerOverHttp.Helpers;
using ModelContextProtocol.Protocol;

var tool = new UiPathRobotToolHandler();
//HashSet<string> subscriptions = [];
var builder = WebApplication.CreateBuilder(args);
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
app.Run("http://0.0.0.0:3002");
