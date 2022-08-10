param(
    [string] $searchServiceName,
    [string] $searchServiceKey,
    [string] $storageAccount,
    [string] $storageKey,
    [string] $storageContainerName,
    [string] $cogServicesKey,
    [string] $dataSourceName,
    [string] $skillsetName,
    [string] $indexName,
    [string] $indexerName
)

$storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=$storageAccount;AccountKey=$storageKey;EndpointSuffix=core.windows.net"


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
                'api-key' = $searchServiceKey
                'Content-Type' = 'application/json' 
                'Accept' = 'application/json' 
            }
            $baseSearchUrl = "https://"+$searchServiceName+".search.windows.net"
            $fullUrl = $baseSearchUrl + $url
        
            Write-Host "Calling api: '"$fullUrl"'";
            Invoke-RestMethod -Uri $fullUrl -Headers $headers -Method Put -Body $body.TrimStart([regex]::Unescape("\ufeff")) | ConvertTo-Json
		}; 

        # Create the datasource  
        $dataSourceBody = (Invoke-WebRequest -Uri "https://raw.githubusercontent.com/Azure-Samples/azure-search-knowledge-mining/main/00%20-%20Resource%20Deployment/templates/base-datasource.json").Content
        $dataSourceBody = $dataSourceBody -replace "{{env_storage_connection_string}}", $storageConnectionString      
        $dataSourceBody = $dataSourceBody -replace "{{env_storage_container}}", $storageContainerName        
        CallSearchAPI -url ("/datasources/"+$dataSourceName+"?api-version=2020-06-30") -body $dataSourceBody

        # Create the skillset
        $skillBody = (Invoke-WebRequest -Uri "https://raw.githubusercontent.com/Azure-Samples/azure-search-knowledge-mining/main/00%20-%20Resource%20Deployment/templates/base-skills.json").Content
        $skillBody = $skillBody -replace "{{cog_services_key}}", $cogServicesKey
        CallSearchAPI -url ("/skillsets/"+$skillsetName+"?api-version=2020-06-30") -body $skillBody

        # Create the index
        $indexBody = (Invoke-WebRequest -Uri "https://raw.githubusercontent.com/Azure-Samples/azure-search-knowledge-mining/main/00%20-%20Resource%20Deployment/templates/base-index.json").Content
        CallSearchAPI -url ("/indexes/"+$indexName+"?api-version=2020-06-30") -body $indexBody
        
        # Create the indexer
        $indexerBody = (Invoke-WebRequest -Uri "https://raw.githubusercontent.com/Azure-Samples/azure-search-knowledge-mining/main/00%20-%20Resource%20Deployment/templates/base-indexer.json").Content
        $indexerBody = $indexerBody -replace "{{datasource_name}}", $dataSourceName
        $indexerBody = $indexerBody -replace "{{skillset_name}}", $skillsetName   
        $indexerBody = $indexerBody -replace "{{index_name}}", $indexName   
        CallSearchAPI -url ("/indexers/"+$indexerName+"?api-version=2020-06-30") -body $indexerBody
	}
    
CreateSearchIndex;
