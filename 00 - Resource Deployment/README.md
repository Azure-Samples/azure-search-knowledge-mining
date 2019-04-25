# Resource Deployment

This folder contains a PowerShell script that can be used to provision the Azure resources required to build your Cognitive Search solution.  You may skip this folder if you prefer to provision your Azure resources via the Azure Portal.  The PowerShell script will provision the following resources to your Azure subscription:

 
| Resource              | Usage                                                                                     |
|-----------------------|-------------------------------------------------------------------------------------------|
| [Azure Search Service](https://azure.microsoft.com/en-us/services/search/)  | The hosting service for the Search Index, Cognitive Skillset, and Search Indexer          |
| [Azure Cognitive Services](https://docs.microsoft.com/en-us/azure/search/cognitive-search-attach-cognitive-services)	| Used by the Cognitive Skills pipeline to process unstructured data	|
|[Azure Storage Account](https://azure.microsoft.com/en-us/services/storage/?v=18.24) | Data source where raw files are stored                                                     |
| [Web App](https://azure.microsoft.com/en-us/services/app-service/web/)               | The hosting service for the Search UI                                                     |
| [Application Insights](https://azure.microsoft.com/en-us/services/monitor/)  | *OPTIONAL* - Telemetry monitoring service for the Search UI									|


By default, this PowerShell script will provision a Basic Search service for your solution. See [Azure Search Pricing](https://azure.microsoft.com/en-us/pricing/details/search/) for information on sizing limits, scaling limits and pricing and choose your desired tier. 

Depending on your custom skill development needs, additional Azure resources may be required.  See the README in the [03 - Data Science & Custom Skills](../03%20-%20Data%20Science%20and%20Custom%20Skills/README.md) folder for additional information.

## Prerequisites
1. Access to an Azure Subscription

## Deploy via Azure Portal
As an alternative to running the PowerShell script, you can deploy the resources manually via the Azure Portal or click the button below to deploy the resources:

<a href="https://azuredeploy.net/?repository=https://github.com/Azure-Samples/azure-search-knowledge-mining" target="_blank">
    <img src="http://azuredeploy.net/deploybutton.png"/>
</a> 

## Steps for Resource Deployment via PowerShell

To run the [PowerShell script](./deploy.ps1):

1. Modify the parameters at the top of **deploy.ps1** to configure the names of your resources and other settings.   
2. Run the [PowerShell script](./deploy.ps1). If you have PowerShell opened to this folder run the command:
`./deploy.ps1`
3. You will then be prompted to login and provide additional information.

### *Notes*

We recommend building an initial prototype solution leveraging a representative subset of data to estimate the size of your final search index.  When you are ready to build your final solution, you will want to size and provision your resources to meet your estimated scale and performance needs.

Please see [Azure Service Limits](https://docs.microsoft.com/en-us/azure/search/search-limits-quotas-capacity) for additional information and best practices on sizing.

