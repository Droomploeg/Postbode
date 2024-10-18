@description('Name of the Service Bus namespace.')
param serviceBusNamespaceName string
@description('Location for all resources.')
param location string


resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
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

resource queues 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = [for index in range(1, 3) : {
  name: 'queue-${index}'
  parent: serviceBusNamespace
  properties: {
    requiresSession: index % 3 == 0 ? true : false
    enablePartitioning: true
  }
}]

resource topics 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = [for index in range(1, 3) : {
      name: 'topic-${index}'
      parent: serviceBusNamespace
  }
]
// resource subscriptions 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = [for index in range(0, 9) : {name:
//   name: 'topic${(index / 3) + 1}/subscription-${(index % 3) + 1}'
//   properties: {
//     enablePartitioning: true
//   }
// ]

output serviceBusNamespaceId string = serviceBusNamespace.id
output serviceBusNamespaceName string = serviceBusNamespace.name
output serviceBusTentantId string = serviceBusNamespace.identity.tenantId
