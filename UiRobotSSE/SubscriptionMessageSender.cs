using Microsoft.Extensions.Hosting;
using ModelContextProtocol;
using ModelContextProtocol.Server;

public class SubscriptionMessageSender(IMcpServer server, HashSet<string> subscriptions) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            /*
            foreach (var uri in subscriptions)
            {
                await server.SendNotificationAsync("notifications/tools/list_changed",
                    new
                    {
                        Uri = uri,
                    }, cancellationToken: stoppingToken);
            }*/
            Console.WriteLine("Sending subscription messages to {0} subscribers", subscriptions.Count);

            await Task.Delay(60000, stoppingToken);
        }
    }
}
