# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Write-Host "Ensuring Azure dependencies are installed."
if (!(Get-Module -Name Az)) {
    Write-Host "Installing Az PowerShell..."
    Install-Module -Name Az
    Import-Module -Name Az
}
if (!(Get-Module -Name Az.Search)) {
    Write-Host "Installing Az.Search PowerShell..."
    Install-Module -Name Az.Search
    Import-Module -Name Az.Search
}

Write-Host @"

------------------------------------------------------------
Guidance for choosing parameters for resource deployment:
 uniqueName: Choose a name that is globally unique and less than 12 characters. This name will be used as a prefix for the resources created and the resultant name must not confict with any other Azure resource. 
   Ex: FabrikamTestPilot1
 
 resourceGroup: Please create a resource group in your Azure account and retreive it's resource group name.
   Ex: testpilotresourcegroup

 subscriptionId: Your subscription id.
   Ex: 123456-7890-1234-5678-9012345678
------------------------------------------------------------

"@

function Deploy
{
    # Read parameters from user.
    Write-Host "Press enter to use [default] value."
    Write-Host "For uniqueName, please enter a string with 10 or less characters."
    Write-Host "For video-indexing provide your Azure Video Indexer account ID. Blank to disable video indexing. https://docs.microsoft.com/en-us/azure/media-services/video-indexer/connect-to-azure"
    
    while (!($uniqueName = Read-Host "uniqueName")) { Write-Host "You must provide a uniqueName."; }
    while (!($resourceGroupName = Read-Host "resourceGroupName")) { Write-Host "You must provide a resourceGroupName."; }
    while (!($subscriptionId = Read-Host "subscriptionId")) { Write-Host "You must provide a subscriptionId."; }

    $defaultLocation = "SouthCentralUS"
    if (!($location = Read-Host "location [$defaultLocation]")) { $location = $defaultLocation }
    $defaultSearchSku = "basic"
    if (!($searchSku = Read-Host "searchSku [$defaultSearchSku]")) { $searchSku = $defaultSearchSku }

    while (!($videoIndexingAccountId = Read-Host "videoIndexingAccountId")) { Write-Host "Azure Video Indexer ID or blank"; }
    If ('' -ne $videoIndexingAccountId) {
        while (!($videoIndexingAccountKey = Read-Host "videoIndexingAccountKey")) { Write-Host "Azure Video Indexer Key"; }
        while (!($videoIndexingLocation = Read-Host "videoIndexingLocation")) { Write-Host "Azure Video Indexer Location"; }
    }

    # Generate derivative parameters.
    $searchServiceName = $uniqueName + "search";
    $webappname = $uniqueName + "app";
    $cogServicesName = $uniqueName + "cog";
    $appInsightsName = $uniqueName + "insights";
    $storageAccountName = $uniqueName + "str";
    $storageContainerName = "documents";
        
    $dataSourceName = $uniqueName + "-datasource";
    $skillsetName = $uniqueName + "-skillset";
    $indexName = $uniqueName + "-index";
    $indexerName = $uniqueName + "-indexer";

    # Only used if we opt-in to index videos
    $videoIndexerName = $uniqueName + "-videoindexer";
    $videoIndexerSkillsetName = $uniqueName + "-videoindexer-skillset";
    $videoIndexerStorageContainerInsightsName = "videoindexerinsights";
    $videoIndexerInsightsIndexerName = $uniqueName + "-videoindexer-insights-indexer";
    $videoIndexerInsightsDataSourceName = $uniqueName + "-videoindexer-insights-datasource";
    $videoIndexerFunctionAppname = $uniqueName + "functions";

    # These values are extracted by this process automatically. Do not set values here.
    $global:storageAccountKey = "";
    $global:searchServiceKey = "";
    $global:storageConnectionString = "";
    $global:cogServicesKey = "";
 
    function ValidateParameters
    {
        Write-Host "------------------------------------------------------------";
        Write-Host "Here are the values of all parameters:";
        Write-Host "uniqueName: '$uniqueName'";
        Write-Host "resourceGroupName: '$resourceGroupName'";
        Write-Host "subscriptionId: '$subscriptionId'";
        Write-Host "location: '$location'";
        Write-Host "searchSku: '$searchSku'";
        Write-Host "searchServiceName: '$searchServiceName'";
        Write-Host "webappname: '$webappname'";
        Write-Host "cogServicesName: '$cogServicesName'";
        Write-Host "appInsightsName: '$appInsightsName'";
        Write-Host "storageAccountName: '$storageAccountName'";
        Write-Host "storageContainerName: '$storageContainerName'";
        Write-Host "dataSourceName: '$dataSourceName'";
        Write-Host "skillsetName: '$skillsetName'";
        Write-Host "indexName: '$indexName'";
        Write-Host "indexerName: '$indexerName'";
        Write-Host "videoIndexerAccountId: '$videoIndexingAccountId'"
        Write-Host "videoIndexerLocation: '$videoIndexingLocation'"
        Write-Host "------------------------------------------------------------";
	}

    ValidateParameters;
 

    function Signin
    {
        # Sign in
        Write-Host "Logging in for '$subscriptionId'";
        Connect-AzAccount;

        # Select subscription
        Write-Host "Selecting subscription '$subscriptionId'";
        Select-AzSubscription -SubscriptionID $subscriptionId;
	}

    Signin;
    
 
    function PrepareSubscription
    {
        # Register RPs
        $resourceProviders = @("microsoft.cognitiveservices", "microsoft.insights", "microsoft.search", "microsoft.storage");
        if ($resourceProviders.length) {
            Write-Host "Registering resource providers"
            foreach ($resourceProvider in $resourceProviders) {
                Register-AzResourceProvider -ProviderNamespace $resourceProvider;
            }
        }
	}

    PrepareSubscription;
    
    
    function FindOrCreateResourceGroup
    {
        # Create or check for existing resource group
        $resourceGroup = Get-AzResourceGroup -Name $resourceGroupName -ErrorAction SilentlyContinue
        if (!$resourceGroup) {
            Write-Host "Resource group '$resourceGroupName' does not exist.";
            if (!$location) {
                $location = Read-Host "please enter a location:";
            }
            Write-Host "Creating resource group '$resourceGroupName' in location '$location'";
            New-AzResourceGroup -Name $resourceGroupName -Location $location
        }
        else {
            Write-Host "Using existing resource group '$resourceGroupName'";
        }
	}

    FindOrCreateResourceGroup;

    
    function CreateStorageAccountAndContainer
    {
        # Create a new storage account
        Write-Host "Creating Storage Account";

        # Create the resource using the API
        $storageAccount = New-AzStorageAccount `
            -ResourceGroupName $resourceGroupName `
            -Name $storageAccountName `
            -Location $location `
            -SkuName Standard_LRS `
            -Kind StorageV2 
        
        $global:storageAccountKey = (Get-AzStorageAccountKey -ResourceGroupName $resourceGroupName -StorageAccountName $storageAccountName)[0].Value        
        $global:storageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=' + $storageAccountName + ';AccountKey=' + $global:storageAccountKey + ';EndpointSuffix=core.windows.net' 
        Write-Host "Storage Account Key: '$global:storageAccountKey'";
                
        $storageContext = New-AzStorageContext `
            -StorageAccountName $storageAccountName `
            -StorageAccountKey $global:storageAccountKey
        
        Write-Host "Creating Storage Container";
        $storageContainer = New-AzStorageContainer `
            -Name $storageContainerName `
            -Context $storageContext `
            -Permission Off

        Write-Host "Creating Video Insights Container";
        $videoInsightsContainer = New-AzStorageContainer `
            -Name $videoIndexerStorageContainerInsightsName `
            -Context $storageContext `
            -Permission Off
    
        Write-Host "Uploading sample_documents directory";
        Push-Location "../sample_documents"
        ls -File -Recurse | Set-AzStorageBlobContent -Container $storageContainerName -Context $storageContext -Force
        Pop-Location
	}

    CreateStorageAccountAndContainer;
    

    

    function CreateSearchServices
    {
        # Create a cognitive services resource
        Write-Host "Creating Cognitive Services";
        $cogServices = New-AzCognitiveServicesAccount `
            -ResourceGroupName $resourceGroupName `
            -Name $cogServicesName `
            -Location $location `
            -SkuName S0 `
            -Type CognitiveServices
        $global:cogServicesKey = (Get-AzCognitiveServicesAccountKey -ResourceGroupName $resourceGroupName -name $cogServicesName).Key1   
        Write-Host "Cognitive Services Key: '$global:cogServicesKey'";
            
        # Create a new search service
        # Alternatively, you can now use the Az.Search module: https://docs.microsoft.com/en-us/azure/search/search-manage-powershell 
        Write-Host "Creating Search Service";
        $searchService = New-AzSearchService  `
            -ResourceGroupName $resourceGroupName `
            -Name $searchServiceName `
            -Sku $searchSku -Location $location `
            -PartitionCount 1 `
            -ReplicaCount 1

        $global:searchServiceKey = (Get-AzSearchAdminKeyPair -ResourceGroupName $resourceGroupName -ServiceName $searchServiceName).Primary         
        Write-Host "Search Service Key: '$global:searchServiceKey'";
	}

    CreateSearchServices;
   
    function CallSearchAPI
    {
        param (
            [string]$url,
            [string]$body
        )

        $headers = @{
            'api-key' = $global:searchServiceKey
            'Content-Type' = 'application/json' 
            'Accept' = 'application/json' 
        }
        $baseSearchUrl = "https://"+$searchServiceName+".search.windows.net"
        $fullUrl = $baseSearchUrl + $url
    
        Write-Host "Calling api: '"$fullUrl"'";
        Invoke-RestMethod -Uri $fullUrl -Headers $headers -Method Put -Body $body | ConvertTo-Json
    }; 

    function CreateSearchIndex
    {
        Write-Host "Creating Search Index"; 
        
        # Create the datasource
        $dataSourceBody = Get-Content -Path .\templates\base-datasource.json  
        $dataSourceBody = $dataSourceBody -replace "{{env_storage_connection_string}}", $global:storageConnectionString      
        $dataSourceBody = $dataSourceBody -replace "{{env_storage_container}}", $storageContainerName        
        CallSearchAPI -url ("/datasources/"+$dataSourceName+"?api-version=2019-05-06") -body $dataSourceBody

        # Create the skillset
        $skillBody = Get-Content -Path .\templates\base-skills.json
        $skillBody = $skillBody -replace "{{cog_services_key}}", $global:cogServicesKey  
        CallSearchAPI -url ("/skillsets/"+$skillsetName+"?api-version=2019-05-06") -body $skillBody

        # Create the index
        $indexBody = Get-Content -Path .\templates\base-index.json 
        CallSearchAPI -url ("/indexes/"+$indexName+"?api-version=2019-05-06") -body $indexBody
        
        # Create the indexer
        $indexerBody = Get-Content -Path .\templates\base-indexer.json
        $indexerBody = $indexerBody -replace "{{datasource_name}}", $dataSourceName
        $indexerBody = $indexerBody -replace "{{skillset_name}}", $skillsetName   
        $indexerBody = $indexerBody -replace "{{index_name}}", $indexName   
        CallSearchAPI -url ("/indexers/"+$indexerName+"?api-version=2019-05-06") -body $indexerBody
	}

    CreateSearchIndex;

    function CreateWebApp
    {
        # Create an App Service plan
        Write-Host "Creating App Service Plan";
        $appService = New-AzAppServicePlan `
            -Name $webappname `
            -Location $location `
            -ResourceGroupName $resourceGroupName `
            -Tier Standard


        # Create a web app.
        Write-Host "Creating Web App";
        $webApp = New-AzWebApp `
            -Name $webappname `
            -Location $location `
            -AppServicePlan $webappname `
            -ResourceGroupName $resourceGroupName `
            -WarningAction SilentlyContinue


        # Create an application insights instance
        Write-Host "Creating App Insights";
        $appInsights = New-AzResource `
            -ResourceName $appInsightsName `
            -ResourceGroupName $resourceGroupName `
            -Tag @{ applicationType = "web"; applicationName = $webappname } `
            -ResourceType "Microsoft.Insights/components" `
            -Location $location `
            -PropertyObject @{"Application_Type" = "web" } `
            -Force


        # Setting App Insights Key in Web app
        Write-Host "Connecting App Insights with Web App";
        $appSetting = @{'APPINSIGHTS_INSTRUMENTATIONKEY' = $appInsights.Properties.InstrumentationKey }
        $global:appInsightsInstrumentationKey =  $appInsights.Properties.InstrumentationKey
        $updateSettings = Set-AzWebApp `
            -Name $webappname `
            -ResourceGroupName $resourceGroupName `
            -AppSettings $appSetting
	}

    CreateWebApp;

    function CreateVideoIndexerFunctionApp
    {
        Write-Host "Creating Funtion App for video indexing function";

        $functionApp = New-AzFunctionApp `
            -ResourceGroupName $resourceGroupName `
            -Name $videoIndexerFunctionAppname `
            -StorageAccountName $storageAccountName `
            -PlanName $webappname `
            -Runtime DotNet `
            -FunctionsVersion 3 `
            -WarningAction SilentlyContinue `
            -ApplicationInsightsKey $global:appInsightsInstrumentationKey `
            -ApplicationInsightsName $appInsightsName `
            -ErrorAction SilentlyContinue

        $token = (Get-AzAccessToken).Token
        $global:skillFunctionCode = (Invoke-RestMethod -Method Post -Uri "https://management.azure.com/subscriptions/$subscriptionId/resourceGroups/$resourceGroupName/providers/Microsoft.Web/sites/$videoIndexerFunctionAppname/functions/video-indexer/listkeys?api-version=2020-12-01"  -Headers @{ Authorization="Bearer $token"; "Content-Type"="application/json"  }).default
        $functionCallbackCode = (Invoke-RestMethod -Method Post -Uri "https://management.azure.com/subscriptions/$subscriptionId/resourceGroups/$resourceGroupName/providers/Microsoft.Web/sites/$videoIndexerFunctionAppname/functions/video-indexer-callback/listkeys?api-version=2020-12-01"  -Headers @{ Authorization="Bearer $token"; "Content-Type"="application/json"  }).default

        $functionAppSettings = @{
            MediaIndexer_AccountId = "$videoIndexingAccountId"
            MediaIndexer_Location = "$videoIndexingLocation"
            MediaIndexer_AccountKey = "$videoIndexingAccountKey"
            MediaIndexer_StorageConnectionString = "$global:storageConnectionString"
            MediaIndexer_StorageContainer = "$videoIndexerStorageContainerInsightsName"
            MediaIndexer_CallbackFunctionCode = $functionCallbackCode
            #https://github.com/projectkudu/kudu/wiki/Deploying-inplace-and-without-repository / https://github.com/projectkudu/kudu/wiki/Customizing-deployments
            PROJECT = "./Video/VideoIndexer/VideoIndexer.csproj"
        }

        Update-AzFunctionAppSetting -ResourceGroupName $resourceGroupName -Name $videoIndexerFunctionAppname -AppSetting $functionAppSettings

        $functionScmProperties = @{
            repoUrl = "https://github.com/azure-samples/azure-search-power-skills";
            branch = "main";
            isManualIntegration = "true";
        }

        Set-AzResource `
            -ResourceGroupName $resourceGroupName `
            -ResourceType Microsoft.Web/sites/sourcecontrols `
            -Name $videoIndexerFunctionAppname/web `
            -PropertyObject  $functionScmProperties `
            -ApiVersion 2020-09-01 `
            -Force

    }

    if (-Not [string]::IsNullOrWhiteSpace($videoIndexingAccountId)) {
        CreateVideoIndexerFunctionApp;
    }

    
    function CreateVideoIndexer
    {
        Write-Host "Creating Video Indexer Index"; 
        
        # Create the video-indexer datasource
        $dataSourceBody = Get-Content -Path .\templates\videoindexer-datasource.json  
        $dataSourceBody = $dataSourceBody -replace "{{env_storage_connection_string}}", $global:storageConnectionString      
        $dataSourceBody = $dataSourceBody -replace "{{env_storage_container}}", $videoIndexerStorageContainerInsightsName        
        CallSearchAPI -url ("/datasources/"+$videoIndexerInsightsDataSourceName+"?api-version=2019-05-06") -body $dataSourceBody

        # Create the skillset
        $skillBody = Get-Content -Path .\templates\videoindexer-skills.json
        $skillBody = $skillBody -replace "{{function_endpoint}}", "https://$($videoIndexerFunctionAppname).azurewebsites.net/"  
        $skillBody = $skillBody -replace "{{function_code}}", "$global:skillFunctionCode"
        CallSearchAPI -url ("/skillsets/"+$videoIndexerSkillsetName+"?api-version=2019-05-06") -body $skillBody
        
        # Create the indexer
        $indexerBody = Get-Content -Path .\templates\videoindexer-indexer.json
        $indexerBody = $indexerBody -replace "{{datasource_name}}", $dataSourceName
        $indexerBody = $indexerBody -replace "{{skillset_name}}", $videoIndexerSkillsetName
        $indexerBody = $indexerBody -replace "{{index_name}}", $indexName   
        CallSearchAPI -url ("/indexers/"+$videoIndexerName+"?api-version=2019-05-06") -body $indexerBody

        # Create the insights document indexer
        $indexerBody = Get-Content -Path .\templates\videoindexer-insights-indexer.json
        $indexerBody = $indexerBody -replace "{{datasource_name}}", $videoIndexerInsightsDataSourceName
        $indexerBody = $indexerBody -replace "{{index_name}}", $indexName
        $indexerBody = $indexerBody -replace "{{schedule_start_time}}", [System.DateTimeOffset]::Now.ToString("u")
        CallSearchAPI -url ("/indexers/"+$videoIndexerInsightsIndexerName+"?api-version=2019-05-06") -body $indexerBody

    }

    if (-Not [string]::IsNullOrWhiteSpace($videoIndexingAccountId)) {
        CreateVideoIndexer;
    }

    function PrintAppsettings
    {
        Write-Host "Copy and paste the following values to update the appsettings.json file described in the next folder:";
        Write-Host "------------------------------------------------------------";
        Write-Host "SearchServiceName: '$searchServiceName'";
        Write-Host "SearchApiKey: '$global:searchServiceKey'";
        Write-Host "SearchIndexName: '$indexName'";
        Write-Host "SearchIndexerName: '$indexerName'";
        Write-Host "StorageAccountName: '$storageAccountName'";
        Write-Host "StorageAccountKey: '$global:storageAccountKey'";
        $StorageContainerAddress = ("https://"+$storageAccountName+".blob.core.windows.net/"+$storageContainerName)
        Write-Host "StorageContainerAddress: '$StorageContainerAddress'";

        if (-Not [string]::IsNullOrWhiteSpace($videoIndexingAccountId)) {
            Write-Host "VideoIndexerAccountId: '$videoIndexingAccountId'";
            Write-Host "VideoIndexerApiKey: '$videoIndexingAccountKey'";
            Write-Host "VideoIndexerLocation: '$videoIndexingLocation'";
        }

        Write-Host "------------------------------------------------------------";
	}
    PrintAppsettings;
}

Deploy;