# DreamOps an Azure ServiceBus Operation Tool
This tool is designed for performing operational tasks on Azure Service Bus. Unlike many other tools, including the well-known Service Bus Explorer, this tool offers advanced authorization at different levels, allowing for effective management of permissions. It operates entirely within Azure.

However, it's important to note that this tool is not intended for DevOps purposes and therefore does not support the creation of queues or topics.

Ideally, no messages should end up in the dead-letter queue of a service bus. However, in reality, this can happen due to factors outside your team's control. This tool helps by grouping messages from the dead-letter queue for analysis, enabling you to focus on manageable outages. Additionally, it allows you to resend messages or send new ones, ensuring efficient message handling.

## Features
The application offers the following features:
* Overview of all queues, topics, and subscriptions
* Status monitoring of queues, topics, and subscriptions
* Resend messages from dead-letter queues
* Send new messages to queues and topics
* Authorization through Azure Entra ID
* Fully operates on Azure

## Getting Started
The tool requires the following components (also if run locally):
* Azure Subscription
* Azure Service Bus
* Azure Entra ID
* Azure Service Principal
* Azure Application Insights (optional)

## Installation on Azure
For the manual a demo application name is used. You can change that is you want.
The demo application name is "DreamOpsDemo01".


### Setting Up Azure Entra ID for the Application

#### Create new app registation
* Open Azure Entra ID (https://entra.microsoft.com)
* Go to _Applications_ -> _App registrations_
* Select _New registration_
* Fill in the form (this is where the demo name is set: _"DreamOpsDemo01"_)
* After creation go to the app registration and set the _Authentication_ <br>
  This is the place for the redirect URIs. Select _Add URI_.
* Fill the URI for redirect "https://<url>/signin-oidc" e.g. 
  * localhost: https://localhost:7273/signin-oidc
  * azure: https://dreamopsdemo01-azurewebsites.net/signin-oidc
* Select checkbox _ID tokens (used for implict and hybrid flows)_
* Press _Save_ 
* Go to _Certificates & Secrets_, we need a secret for our application
* Select tab _Client secrets_ and create the secret this is needed for the appsettings
  (secret will be called _ClientSecret_ in the appsettings)
* Go to _App roles_ and create the roles. In DreamOps has only one role _General_Access_.
* Select _Create app role_ and set the following field:
  * Display name: _General_Access_
  * Allowed member types: _Users/Groups_ 
  * Value: _General_Access_
  * Description: _General access for DreamOps_
  * _Check_ Do you want to enable this app role?
* Press _Apply_

The application is created in EntraId this is needed for running in Azure or on localhost.

#### Enable access for Users/Groups
* Open Azure Entra ID (https://entra.microsoft.com)
* Go to _Applications_ -> _Enterprise applications_
* Select _your application_
* Select _Users and Groups_, here you can set the users/groups that have acces to the DreamOps application.
* Select _Add user/group_ and select a User/Group and the Role.
* Press _Assign_ and the user/group is assigned with the role.

The user/group has access to the DreamOps application.