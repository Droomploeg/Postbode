param appName string = 'postbode'
param company string  
param environment string = 'dev' //dev, test, acc, prod

param location string = 'westeurope'
param keyVaultAllowedIpRules string  = '0.0.0.0/32'
param resourcePrefix string = '${environment}-${company}-${appName}'
param resourceGroupName string = '${resourcePrefix}-rg01'

var ipRules = keyVaultAllowedIpRules == '' ? [] : split(keyVaultAllowedIpRules, ',')

targetScope = 'subscription'

resource resourceGroup 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
}

module vnet 'networks/vnet.bicep' = {
  name: 'createVnet'
  scope: resourceGroup
  params: {
    vnetName: '${resourcePrefix}-vnet01'
    location: location
    ipRange: '10.0.0.0/16'
    subnetName: '${resourcePrefix}-subnet01'
    ipSubnetRange: '10.0.1.0/24'
    serviceEndpoint: [
      {
        service: 'Microsoft.KeyVault'
      } 
      {
        service: 'Microsoft.Web'
      }
    ]
  }
}

module userAssignedIdentities 'managedIdentities/userAssignedIdentities.bicep' = {
  name: 'createUserAssignedIdentity'
  scope: resourceGroup
  params: {
    identityName: '${resourcePrefix}-mi01'
    location: location
  }
}

module appInsight 'applicationInsights/applicationInsight.bicep' = {
  name: 'createAppInsight'
  scope: resourceGroup
  params: {
    applicationInsightsName: '${resourcePrefix}-ai01'
    logAnalyticsWorkspaceName: '${resourcePrefix}-law01'
    location: location
  }
}

module keyVault 'keyvault/keyvault.bicep' = {
  name: 'createKeyVault'
  scope: resourceGroup
  params: {
    keyVaultName: '${resourcePrefix}-kv01'
    location: location
    ipRules: ipRules
    subnetId: vnet.outputs.subnetId
  }
  dependsOn: [
    vnet
    userAssignedIdentities
  ]
}

module serviceBus 'servicebus/servicebus.bicep' = {
  name: 'createServiceBus'
  scope: resourceGroup
  params: {
    serviceBusNamespaceName: '${resourcePrefix}-sb01'
    location: location
  }
}

module appServicePlan 'appServices/appServicePlan.bicep' = {
  name: 'createAppServicePlan'
  scope: resourceGroup
  params: {
    appServicePlanName: '${resourcePrefix}-asp01'
    location: location
  }
  dependsOn: [
    vnet
  ]
}

module webApp 'appServices/webapp.bicep' = {
  name: 'createWebApp'
  scope: resourceGroup
  params: {
    appName: '${resourcePrefix}-wap01'
    location: location
    appServicePlanId: appServicePlan.outputs.appServicePlanId
    appInsightsName: appInsight.outputs.appInsightsName
    identityName: userAssignedIdentities.outputs.identityName
    keyVaultName: keyVault.outputs.keyVaultName
    subnetId: vnet.outputs.subnetId
  }
  dependsOn: [
    appInsight
    appServicePlan
    keyVault
    userAssignedIdentities
    vnet
    serviceBus
  ]
}

module assignWebAppRole 'keyvault/assignRole.bicep' = {
  name: 'assignWebAppRole'
  scope: resourceGroup
  params: {
    role: 'KeyVaultOfficer'
    principalId: userAssignedIdentities.outputs.principalId
    keyVaultName: keyVault.outputs.keyVaultName
  }
  dependsOn: [
    keyVault
    webApp
  ]
}

module assignServiceBusRole 'servicebus/assignRole.bicep' = {
  name: 'assignServiceBusRole'
  scope: resourceGroup  
  params: {
    role: 'Owner'
    principalId: userAssignedIdentities.outputs.principalId
    serviceBusNamespaceName: serviceBus.outputs.serviceBusNamespaceName
  }
  dependsOn: [
    serviceBus
    webApp
  ]
}

output resourceGroupName string = resourceGroup.name
output keyVaultName string = keyVault.outputs.keyVaultName
output webAppName string = webApp.outputs.webAppName
output serviceBusNamespaceName string = serviceBus.outputs.serviceBusNamespaceName

