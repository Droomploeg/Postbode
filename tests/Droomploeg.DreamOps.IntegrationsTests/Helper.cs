using Azure.Messaging.ServiceBus.Administration;
using Bunit;
using Droomploeg.DreamOps.Infrastructure.AzureServiceBus;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;

namespace Droomploeg.DreamOps.IntegrationsTests;

internal class Helper
{
    private static async Task PrepareAsync(TestContext context)
    {
        var clientContext = context.Services.GetRequiredService<IServiceBusInfoContext>();
        var adminFactoryClient = context.Services.GetRequiredService<IAzureClientFactory<ServiceBusAdministrationClient>>();
        var adminClient = adminFactoryClient.CreateClient(clientContext.Current.Name);
        var response = adminClient.GetQueuesAsync().GetAsyncEnumerator();

        // delete all queues
        while (await response.MoveNextAsync())
        {
            var queue = response.Current;
            await adminClient.DeleteQueueAsync(queue.Name);
        }

        // check if all queues are deleted
        var queues = adminClient.GetQueuesAsync().GetAsyncEnumerator();
        while (await queues.MoveNextAsync())
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            queues = adminClient.GetQueuesAsync().GetAsyncEnumerator();
        }

        // create queues
        await adminClient.CreateQueueAsync("detail-send-nosession");
        await adminClient.CreateQueueAsync("detail-refresh-nosession");

        await adminClient.CreateQueueAsync("detail-active-peek-nosession");
        await adminClient.CreateQueueAsync("detail-active-delete-nosession");
        await adminClient.CreateQueueAsync("detail-active-deadletter-nosession");
        await adminClient.CreateQueueAsync("detail-active-deleteAll-nosession");

        await adminClient.CreateQueueAsync("detail-deadletter-peek-nosession");
        await adminClient.CreateQueueAsync("detail-deadletter-delete-nosession");
        await adminClient.CreateQueueAsync("detail-deadletter-deleteAll-nosession");
        await adminClient.CreateQueueAsync("detail-deadletter-resubmit-nosession");
        await adminClient.CreateQueueAsync("detail-deadletter-resubmitAll-nosession");

        await adminClient.CreateQueueAsync("overview-refresh");
    }


}
