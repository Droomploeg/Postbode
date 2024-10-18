@description('Name of the key vault')
param keyVaultName string
@description('Location for all resources.')
param location string
@description('Array of IP addresses to allow access to the key vault')
param ipRules array = []
@description('SubnetId')
param subnetId string


resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    accessPolicies:[]
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: false
    networkAcls: {
      bypass: 'None'
      defaultAction: 'Deny'
      ipRules: [
        for ip in ipRules: {
          value: ip
          }
      ]
      virtualNetworkRules: [
        {
          id: subnetId
        }
      ] 
    }
  }
}

output keyVaultName string = keyVault.name
output keyVaultId string = keyVault.id

