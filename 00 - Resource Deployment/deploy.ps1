# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

###### PARAMETERS SECTION #####
$resourceGroupName = "";
$subscriptionId = "";
$location = "SouthCentralUS";
$searchSku = "basic"; # "free", "basic", or "standard"

# Resource names should be lower case
$searchServiceName = "";
$webappname = "";
$cogServicesName = "";
$appInsightsName = $webappname + "insights";

# Storage account names can only include numbers and letters
$storageAccountName = "";

###### END PARAMETERS SECTION #####


# sign in
Write-Host "Logging in...";
Login-AzureRmAccount;

# select subscription
Write-Host "Selecting subscription '$subscriptionId'";
Select-AzureRmSubscription -SubscriptionID $subscriptionId;


# Register RPs
$resourceProviders = @("microsoft.cognitiveservices", "microsoft.documentdb","microsoft.insights","microsoft.search","microsoft.sql","microsoft.storage");
if($resourceProviders.length) {
    Write-Host "Registering resource providers"
    foreach($resourceProvider in $resourceProviders) {
        Register-AzureRmResourceProvider -ProviderNamespace $resourceProvider;
    }
}


#Create or check for existing resource group
$resourceGroup = Get-AzureRmResourceGroup -Name $resourceGroupName -ErrorAction SilentlyContinue
if(!$resourceGroup)
{
    Write-Host "Resource group '$resourceGroupName' does not exist.";
    if(!$location) {
        $location = Read-Host "please enter a location:";
    }
    Write-Host "Creating resource group '$resourceGroupName' in location '$location'";
    New-AzureRmResourceGroup -Name $resourceGroupName -Location $location
}
else{
    Write-Host "Using existing resource group '$resourceGroupName'";
}


# Create a new storage account
Write-Host "Creating Storage Account";
$storageAccount = New-AzureRmStorageAccount `
  -ResourceGroupName $resourceGroupName `
  -Name $storageAccountName `
  -Location $location `
  -SkuName Standard_LRS `
  -Kind StorageV2 


# Create a cognitive services resource
Write-Host "Creating Cognitive Services";
$cogServices = New-AzureRmCognitiveServicesAccount `
  -ResourceGroupName $resourceGroupName `
  -Name $cogServicesName `
  -Location $location `
  -SkuName S0 `
  -Type CognitiveServices 


# Create a new search service
# This command will return once the service is fully created
Write-Host "Creating Search Service";
$searchService = New-AzureRmResourceGroupDeployment `
    -ResourceGroupName $resourceGroupName `
    -TemplateUri "https://gallery.azure.com/artifact/20151001/Microsoft.Search.1.0.9/DeploymentTemplates/searchServiceDefaultTemplate.json" `
    -NameFromTemplate $searchServiceName `
    -Sku $searchSku `
    -Location $location `
    -PartitionCount 1 `
    -ReplicaCount 1


# Create an App Service plan
Write-Host "Creating App Service Plan";
$appService = New-AzureRmAppServicePlan `
   -Name $webappname `
   -Location $location `
   -ResourceGroupName $resourceGroupName `
   -Tier Standard



# Create a web app.
Write-Host "Creating Web App";
$webApp = New-AzureRmWebApp `
   -Name $webappname `
   -Location $location `
   -AppServicePlan $webappname `
   -ResourceGroupName $resourceGroupName `
   -WarningAction SilentlyContinue



# Create an application insights instance
Write-Host "Creating App Insights";
$appInsights = New-AzureRmResource `
  -ResourceName $appInsightsName `
  -ResourceGroupName $resourceGroupName `
  -Tag @{ applicationType = "web"; applicationName = $webappname} `
  -ResourceType "Microsoft.Insights/components" `
  -Location $location `
  -PropertyObject @{"Application_Type"="web"} `
  -Force


# Setting App Insights Key in Web app
Write-Host "Connecting App Insights with Web App";
$appSetting = @{'APPINSIGHTS_INSTRUMENTATIONKEY'= $appInsights.Properties.InstrumentationKey}
$updateSettings = Set-AzureRmWebApp -Name $webappname  -ResourceGroupName $resourceGroupName -AppSettings $appSetting

