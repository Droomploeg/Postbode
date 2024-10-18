@description('Assigns a role to a principal on a Service Bus namespace')
param serviceBusNamespaceName string
@description('The principal id to assign the role to')
param principalId string

@allowed([
  'Reader' 
  'Sender'
  'Owner'
])
param role string

var readerRoleId = '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0'// Azure Service Bus Data Receiver
var senderRoleId = '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39'// Azure Service Bus Data Sender
var ownerRoleId = '090c5cfd-751d-490a-894a-3ce6f1109419'// Azure Service Bus Data Owner
var roleDefinitionId = role == 'Reader' ? readerRoleId : role == 'Sender' ? senderRoleId : ownerRoleId

resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' existing = {
  name: serviceBusNamespaceName
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: serviceBus
  name: guid(serviceBus.id, principalId, roleDefinitionId)
  properties: {
    principalId: principalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionId)
  }
}
