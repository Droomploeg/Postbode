@description('private endpoint name')
param privateEndpointName string
@description('keyvault id')
param keyVaultId string
@description('subnet id')
param subnetId string

resource privateEndpoint 'Microsoft.Network/privateEndpoints@2021-03-01' = {
  name: privateEndpointName
  location: resourceGroup().location
  properties: {
    subnet: {
      id: subnetId
    }
    privateLinkServiceConnections: [
      {
        name: 'keyvault-connection'
        properties: {
          privateLinkServiceId: keyVaultId
          groupIds: [
            'vault'
          ]
        }
      }
    ]
  }
}
