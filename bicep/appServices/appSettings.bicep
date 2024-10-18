@description('app settings for the web app')
param appSettings object
@description('Name of the web app')
param webAppName string

resource webApp 'Microsoft.Web/sites@2023-12-01' existing = {
  name: webAppName
}

resource webAppConfig 'Microsoft.Web/sites/config@2023-12-01' = {
  name: 'appsettings'
  parent: webApp
  properties: appSettings
}

