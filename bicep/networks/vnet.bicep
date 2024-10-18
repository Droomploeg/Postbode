@description('Create a Virtual Network with a subnet')
param vnetName string
@description('Location for the resource.')
param location string
@description('Name of the subnet')
param ipRange string = '10.0.0.0/16'
@description('The address prefix for the subnet')
param subnetName string
@description('The arange for the subnet')
param ipSubnetRange string = '10.0.1.0/24'
@description('The service endpoints for the subnet')
param serviceEndpoint object[]


resource vnet 'Microsoft.Network/virtualNetworks@2020-11-01' = {
  name: vnetName
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        ipRange
      ]
    }
    subnets: [
      {
        name: subnetName
        properties: {
          addressPrefix: ipSubnetRange
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
    ]
  }
}

resource subnet 'Microsoft.Network/virtualNetworks/subnets@2024-01-01' existing =  {
  name: subnetName
  parent: vnet
}

output vnetId string = vnet.id
output vnetName string = vnet.name
output subnetId string = subnet.id
