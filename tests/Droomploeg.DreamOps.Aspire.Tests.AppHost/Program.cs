var builder = DistributedApplication.CreateBuilder(args);

var serviceBus = builder.AddAzureServiceBus("servicebus")
    .RunAsEmulator();

serviceBus.AddServiceBusQueue("test-queue");
serviceBus.AddServiceBusQueue("test-send-message");
serviceBus.AddServiceBusQueue("test-peek-active");
serviceBus.AddServiceBusQueue("test-peek-empty");
serviceBus.AddServiceBusQueue("test-delete-message");
serviceBus.AddServiceBusQueue("test-deadletter-message");
serviceBus.AddServiceBusQueue("test-peek-deadletter");
serviceBus.AddServiceBusQueue("test-resubmit-deadletter");
serviceBus.AddServiceBusQueue("test-delete-deadletter");
serviceBus.AddServiceBusQueue("test-runtime-info");
serviceBus.AddServiceBusQueue("test-send-multiple");
serviceBus.AddServiceBusQueue("test-delete-all");

// Topics + subscriptions for SubscriptionDetailPage tests (one per test class for parallel isolation)
serviceBus.AddServiceBusTopic("test-topic-peek");
serviceBus.AddSubscription("test-topic-peek", "sub-peek");
serviceBus.AddServiceBusTopic("test-topic-delete");
serviceBus.AddSubscription("test-topic-delete", "sub-delete");
serviceBus.AddServiceBusTopic("test-topic-deadletter");
serviceBus.AddSubscription("test-topic-deadletter", "sub-deadletter");
serviceBus.AddServiceBusTopic("test-topic-deleteall");
serviceBus.AddSubscription("test-topic-deleteall", "sub-deleteall");
serviceBus.AddServiceBusTopic("test-topic-resubmitall");
serviceBus.AddSubscription("test-topic-resubmitall", "sub-resubmitall");
serviceBus.AddServiceBusTopic("test-topic-send");
serviceBus.AddSubscription("test-topic-send", "sub-send");
serviceBus.AddServiceBusTopic("test-topic-delete-dl");
serviceBus.AddSubscription("test-topic-delete-dl", "sub-delete-dl");
serviceBus.AddServiceBusTopic("test-topic-resubmit");
serviceBus.AddSubscription("test-topic-resubmit", "sub-resubmit");
serviceBus.AddServiceBusQueue("test-select-message");
serviceBus.AddServiceBusTopic("test-topic-select");
serviceBus.AddSubscription("test-topic-select", "sub-select");
serviceBus.AddServiceBusQueue("test-queue-session").WithProperties(q => q.RequiresSession = true);
var sessionTopic = serviceBus.AddServiceBusTopic("test-topic-session");
sessionTopic.AddServiceBusSubscription("sub-session").WithProperties(s => s.RequiresSession = true);

if (!builder.ExecutionContext.IsRunMode || args.Contains("--with-webapp"))
{
    builder.AddProject<Projects.Droomploeg_DreamOps_WebApp>("webapp")
        .WithReference(serviceBus)
        .WaitFor(serviceBus);
}

builder.Build().Run();
