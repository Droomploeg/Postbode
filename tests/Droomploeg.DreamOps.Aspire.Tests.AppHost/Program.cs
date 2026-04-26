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
serviceBus.AddServiceBusTopic("test-topic-peek").AddServiceBusSubscription("sub-peek");
serviceBus.AddServiceBusTopic("test-topic-delete").AddServiceBusSubscription("sub-delete");
serviceBus.AddServiceBusTopic("test-topic-deadletter").AddServiceBusSubscription("sub-deadletter");
serviceBus.AddServiceBusTopic("test-topic-deleteall").AddServiceBusSubscription("sub-deleteall");
serviceBus.AddServiceBusTopic("test-topic-resubmitall").AddServiceBusSubscription("sub-resubmitall");
serviceBus.AddServiceBusTopic("test-topic-send").AddServiceBusSubscription("sub-send");
serviceBus.AddServiceBusTopic("test-topic-delete-dl").AddServiceBusSubscription("sub-delete-dl");
serviceBus.AddServiceBusTopic("test-topic-resubmit").AddServiceBusSubscription("sub-resubmit"); 
serviceBus.AddServiceBusQueue("test-select-message");
serviceBus.AddServiceBusTopic("test-topic-select").AddServiceBusSubscription("sub-select");
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
