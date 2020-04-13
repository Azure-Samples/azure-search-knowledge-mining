# Resource Deployment

There are two options for deploying the resources to Azure for this solution accelerator:

1. **Using a PowerShell Script**: `deploy.ps1`

    This script is the fastest way to get your solution up and running and will perform the following actions:

    1. Provision the required Azure resources
    2. Upload sample data to your storage account
    3. Create a search index
    4. Print out the values and keys needed for the web app's *appsettings.json*

    If you choose to run this script, you can skip the Search Index Creation in the next folder.

2. **Using an ARM Template**: `azuredeploy.json`

    To deploy this ARM Template, simply press the button below:

    > Please note that this will only deploy the resources. You'll then need to create a search index in the next step.

    <a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fazure-search-knowledge-mining%2Fmaster%2Fazuredeploy.json" target="_blank">
        <img src="http://azuredeploy.net/deploybutton.png"/>
    </a>

## Prerequisites

1. Access to an Azure Subscription

## Running the PowerShell Script

> If you're new to PowerShell, you can follow the instructions on [How to run PowerShell script file on Windows 10](https://www.windowscentral.com/how-create-and-run-your-first-powershell-script-file-windows-10) to help you get started.

To run the [PowerShell script](./deploy.ps1):

1. Open PowerShell and navigate to this folder.
2. Run the following command:

    ```cmd
        ./deploy.ps1
    ```

3. After running the script, you'll be prompted to login and provide additional information.

## Resources Deployed

The PowerShell script will provision the following resources to your Azure subscription:

| Resource              | Usage                                                                                     |
|-----------------------|-------------------------------------------------------------------------------------------|
| [Azure Search Service](https://azure.microsoft.com/en-us/services/search/)  | The hosting service for the Search Index, Cognitive Skillset, and Search Indexer          |
| [Azure Cognitive Services](https://docs.microsoft.com/en-us/azure/search/cognitive-search-attach-cognitive-services)	| Used by the Cognitive Skills pipeline to process unstructured data	|
|[Azure Storage Account](https://azure.microsoft.com/en-us/services/storage/?v=18.24) | Data source where raw files are stored                                                     |
| [Web App](https://azure.microsoft.com/en-us/services/app-service/web/)               | The hosting service for the Search UI                                                     |
| [Application Insights](https://azure.microsoft.com/en-us/services/monitor/)  | Telemetry monitoring service for the Search UI (*Optional*)									|

By default, this PowerShell script will provision a Basic Search service for your solution. See [Azure Search Pricing](https://azure.microsoft.com/en-us/pricing/details/search/) for information on sizing limits, scaling limits and pricing and choose your desired tier. 

Depending on your custom skill development needs, additional Azure resources may be required.  See the README in the [03 - Data Science & Custom Skills](../03%20-%20Data%20Science%20and%20Custom%20Skills/README.md) folder for additional information.

## Notes

We recommend building an initial prototype solution leveraging a representative subset of data to estimate the size of your final search index.  When you are ready to build your final solution, you will want to size and provision your resources to meet your estimated scale and performance needs.

Please see [Azure Service Limits](https://docs.microsoft.com/en-us/azure/search/search-limits-quotas-capacity) for additional information and best practices on sizing.