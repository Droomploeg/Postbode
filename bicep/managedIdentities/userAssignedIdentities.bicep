@description('Name of the identity')
param identityName string

@description('Location for all resources.')
param location string

resource userAssignedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: identityName
  location: location
}


output identityId string = userAssignedIdentity.id
output tenantId string = userAssignedIdentity.properties.tenantId
output principalId string = userAssignedIdentity.properties.principalId
output clientId string = userAssignedIdentity.properties.clientId
output identityName string = userAssignedIdentity.name
