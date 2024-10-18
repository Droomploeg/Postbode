@description('Create a subnet in an existing virtual network')
param vnetName string
@description('Name of the subnet')
param subnetName string
@description('The address prefix for the subnet')
param range string 
@description('The service endpoints for the subnet')
param serviceEndpoint object[]

resource vnet 'Microsoft.Network/virtualNetworks@2024-01-01' existing = {
  name: vnetName
}

resource subnet 'Microsoft.Network/virtualNetworks/subnets@2024-01-01' =  {
  name: subnetName
  parent: vnet
  properties: {
    addressPrefix: range
    serviceEndpoints: serviceEndpoint
    delegations: [
      {
        name: 'appServiceDelegation'
        properties: {
          serviceName: 'Microsoft.Web/serverFarms'
        }
      }
    ]
  }
}

output subnetId string = subnet.id
