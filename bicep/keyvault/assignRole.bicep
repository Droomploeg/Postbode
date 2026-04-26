@description('Assigns a role to a principal on a key vault')
param keyVaultName string
@description('The principal id to assign the role to')
param principalId string

@allowed([
  'KeyVaultAdministrator' 
  'KeyVaultOfficer'
])
param role string

var keyVaultAdministrator = '00482a5a-887f-4fb3-b363-3b7fe8e74483'// Key Vault Administrator
var keyVaultOfficer = 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7' // Key Vault Officer
var roleDefinitionId = role == 'KeyVaultAdministrator' ? keyVaultAdministrator : keyVaultOfficer

resource keyvault 'Microsoft.KeyVault/vaults@2024-04-01' existing = {
  name: keyVaultName
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: keyvault
  name: guid(keyvault.id, principalId, roleDefinitionId)
  properties: {
    principalId: principalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionId)
  }
}
