# Creating a Search Index with PowerShell

The `deploy.ps1` PowerShell script in the previous step shows how to create a search index

The following code will create a search index for you based on the json files in the `templates` folder:

```powershell
    while (!($uniqueName = Read-Host "uniqueName")) { Write-Host "You must provide a uniqueName."; }

    # Generate derivative parameters.
    $searchServiceName = $uniqueName + "search";
    $storageAccountName = $uniqueName + "str";
    $storageContainerName = "documents";

    $dataSourceName = $uniqueName + "-datasource";
    $skillsetName = $uniqueName + "-skillset";
    $indexName = $uniqueName + "-index";
    $indexerName = $uniqueName + "-indexer";

    $global:storageAccountKey = "";
    $global:searchServiceKey = "";
    $global:storageConnectionString = "";
    $global:cogServicesKey = "";

    function CreateSearchIndex
    {
        Write-Host "Creating Search Index"; 

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
```

## Prerequisites

2. To run the PowerShell script, you'll need to install the [Az PowerShell Module](https://docs.microsoft.com/powershell/azure/install-az-ps)

## Running the PowerShell Script

> If you're new to PowerShell, you can follow the instructions on [How to run PowerShell script file on Windows 10](https://www.windowscentral.com/how-create-and-run-your-first-powershell-script-file-windows-10) to help you get started.

To run the [PowerShell script](./deploy.ps1):

1. Open PowerShell and navigate to this folder.

    ```cmd
    cd "00 - Resource Deployment"
    ```

2. Run the following command:

    ```cmd
    ./deploy.ps1
    ```

3. After running the script, you'll be prompted to login and provide additional information.
