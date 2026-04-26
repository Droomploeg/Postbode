@description('Create a web app with .NET 10.0 runtime.')
param appName string
@description('Location for all resources.')
param location string
@description('App Service Plan ID')
param appServicePlanId string
@description('Name of the user assigned identity')
param identityName string 
@description('AppInsights name')
param appInsightsName string
@description('Keyvault Name')
param keyVaultName string
@description('SubnetId')
param subnetId string 

resource userAssignedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: identityName
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: appInsightsName
}

var userAssignedIdentityId = userAssignedIdentity.id

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: appName
  location: location
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    vnetRouteAllEnabled: true
    siteConfig: {
      alwaysOn: true
      linuxFxVersion: 'DOTNETCORE|10.0'
      minTlsVersion: '1.3'
      virtualApplications: [
        {
          virtualPath: '/'
          physicalPath: 'site\\wwwroot'
          preloadEnabled: true
        }
      ]
    }
    virtualNetworkSubnetId: subnetId
    
    reserved: true
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentityId}': {}
    }
  }
}

// this is needed to ensure that the app settings are not overwritten
var defaultAppSettings = {
  WEBSITE_RUN_FROM_PACKAGE: '1'
  WEBSITE_VNET_ROUTE_ALL: '1'
  Logging__LogLevel__Default: 'Information'
  'Logging__LogLevel__Microsoft.AspNetCore': 'Warning'
  AZURE_CLIENT_ID: userAssignedIdentity.properties.clientId
  ASPNETCORE_ENVIRONMENT: 'Production'
  APPLICATIONINSIGHTS_CONNECTION_STRING: appInsights.properties.ConnectionString
  ApplicationInsightsAgent_EXTENSION_VERSION: '~3'
  XDT_MicrosoftApplicationInsights_Mode: 'recommended'  
  AzureKeyVault: 'https://${keyVaultName}${environment().suffixes.keyvaultDns}'
}

var originalAppSettings = list('${webApp.id}/config/appsettings', '2023-12-01').properties

module functionAppSettingsModule 'appSettings.bicep' = {
  name: '${webApp.name}-appsettings_deployment'
  params: {
    webAppName: webApp.name
    appSettings: union(originalAppSettings, defaultAppSettings)
  }
}

output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
output webAppName string = webApp.name
