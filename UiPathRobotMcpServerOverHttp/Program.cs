
using UiPath.Robot.Api;
using UiPathRobotMcpServerOverHttp.Helpers;

var tool = new UiPathRobotToolHandler();
//HashSet<string> subscriptions = [];
var builder = WebApplication.CreateBuilder(args);
var mcpserverbuider = builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithListToolsHandler(tool.UiPathRobotListHandler)
    .WithCallToolHandler(tool.UiPathRobotToolCallHandler);

/*
builder.Services.AddSingleton(subscriptions);
builder.Services.AddHostedService<SubscriptionMessageSender>();
*/
var app = builder.Build();

app.MapMcp();

//app.UsePathBase("/sse");
app.Run("http://127.0.0.1:3001");
