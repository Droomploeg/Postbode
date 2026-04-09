@description('Name of the Service Bus namespace.')
param serviceBusNamespaceName string
@description('Location for all resources.')
param location string


resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2024-01-01' = {
  name: serviceBusNamespaceName
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  identity: {
    type: 'SystemAssigned'
  }
}

resource queues 'Microsoft.ServiceBus/namespaces/queues@2024-01-01' = [for index in range(1, 3) : {
  name: 'queue-${index}'
  parent: serviceBusNamespace
  properties: {
    requiresSession: index % 3 == 0 ? true : false
    enablePartitioning: true
  }
}]

resource topics 'Microsoft.ServiceBus/namespaces/topics@2024-01-01' = [for index in range(1, 3) : {
  name: 'topic-${index}'
  parent: serviceBusNamespace
}]

output serviceBusNamespaceId string = serviceBusNamespace.id
output serviceBusNamespaceName string = serviceBusNamespace.name
output serviceBusTenantId string = serviceBusNamespace.identity.tenantId
