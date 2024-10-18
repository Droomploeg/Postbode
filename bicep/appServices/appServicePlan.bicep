@description('Create an App Service Plan for the DreamOps web app')
param appServicePlanName string
@description('Location for all resources.')
param location string

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  kind: 'linux'
  sku: {
    name: 'B1'
    capacity: 1
  }
  properties: {
    reserved: true
  }
}

output appServicePlanId string = appServicePlan.id
