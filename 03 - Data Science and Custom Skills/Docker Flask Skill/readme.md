# Custom Skill Built using Docker, Python and Flask 

The purpose of this skill is to show how you can leverage Python code as part of the Cognitive Search ingestion in the form of a custom skill.  Since this is hosted in a Docker container, the service can be easily scaled using Kubernetes.  This readme will walk through how to setup and configure the docker instance to run this skill and then how to hook it up as a Custom Skill.

If there is Python code that you wish to leverage as part of Cognitive Search, this would be a good starting point. This walkthrough assumes you are familiar with setting up and using Python.

## What Does this Skill Do?

The purpose of this skill is to take text and extract key phrases from it.  It does this by leveraging Spacy and NLTK to find the Part of Speech for each term in the content, extract important ones such as nouns and then return them.  For example, if the skill receieved the text "The quick brown fox jumped over the lazy dog.", the resulting key phrases would be "keyphrases": ["lazy dog","quick brown fox"].

## Getting Started

The first thing you will want to do is to build the Docker container.  To do this, you will need:

* [Docker Desktop](https://www.docker.com/products/docker-desktop) installed 
* [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest)
* An Azure subscription where we will be hosting the docker container and leveraging Azure Search from
* The files from this Github directory downloaded locally
* Azure Blob Storage with some text files (although with some minor changes this could be files like PDF or Office docs)

## Build the Docker Image

From your desktop open "Windows Azure Command Prompt" and go to the directory you downloaded the Github files, of which you should have: 
* ws.py: Hosting the python code which is run as a Flask service.  
* requirements.txt: Includes all the python packages used
* DockerFile: Details on how to build the docker image

Type: <code>docker.exe build -t kpe .</code>

Note, this will take a while as there is a lot to download

## Run the Docker Container Locally

Next, we will test the image locally to make sure it works as expected.  Assuming you have nothing else running on port 8080, you can start the container:

Type: <code>docker.exe run -p 8080:80 kpe</code>

## Test the Docker Container Locally

There are a lot of tools for testing APis, such as [Postman](https://www.getpostman.com/), or CURL.  Here is an example of a request to test the container:

POST: http://localhost:8080/process

Header: 
<code>Content-Type: application/json</code>

Body (raw): 

```json
{
   "values": [
        {
        	"recordId": "a5",
        	"data":
	        {
        	   "metadata_storage_path": "a125",
	           "text":  "The quick brown fox jumped over the lazy dog."
	        }
        }
   ]
}
```

You should get a response:

```json
{
    "values": [
        {
            "data": {
                "keyphrases": [
                    "lazy dog",
                    "quick brown fox"
                ]
            },
            "recordId": "a5"
        }
    ]
}
```

## Upload Docker Image to Azure 

We will be uploading the docker image to the Azure Container Registry.  In a subsequent step we will be deploying the docker container to an Azure Web App, however this registry has the nice aspect that it allows us to do contiunous deployment so whenever we add an updated container to the registry it will automatically update the running container.

* [Create an Azure Container registry ](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-get-started-portal)
* Once the registry is created, open it "Access Keys" in the Settings and make note of the Login Server, Username and one of the passwords
* From your desktop in the Azure Command Prompt login to your Azure Subscription by typing: <code>az login</code>
* Log in to your Azure container registry by typing: <code>docker login [Login Server]</code> where [Login Server] is the value taken from the above step.  If this is the first time logging in to your container registry, you will need to enter the above username and password.
* Tag the local container: <code>docker tag kpe [Login Server]/samples/kpe</code>
* Upload the tagged container: <code>docker push [Login Server]/samples/kpe</code>

## Deploy the Docker Image to an Azure Web App

There are numerous ways to run a docker container in Azure, however, I prefered to use Azure Web Apps, because it allowed me to add an HTTPS endpoint which is required by Cognitive Search. To do this: 

* From Azure Portal, create a "Web App", which is located under the "Web" section of the Azure Marketplace.  Make sure it is in the sam subscription as that of your Azure Container Registry.
* In the Basics configuration tab, enter the details, ensuring:
    * Publish: Docker Image
    * Operating System: Linux
    * App service plan need to be one that allows for HTTPS (I used Basic B1)
* In the Basics configuration tab, enter the details, ensuring:
    * Image Source: Azure Container Registry
    * Publish: Docker Image
    * Registry ==> Choose your Azure Container Registry
    * Image ==> Choose your Azure Container Image
    * Tag: Latest

## Wait for Image to come Online
Just because the Web App is created, does not mean the container is ready.  To monitor the progress:
* In the portal for the Web App you just created, choose "Container Settings" and in the Logs, scroll down to the bottom.  

You will see a lot of lines indicating it is downloading the image.  Keep choosing "Refresh" until you see something like:
<code>
2019-06-12 23:32:14.243 INFO  - Initiating warmup request to container XXX-container-kp_0 for site liamca-container-kp

2019-06-12 23:32:30.573 INFO  - Container XXX-container-kp_0 for site liamca-container-kp initialized successfully and is ready to serve requests.
</code>

* Optional: I like to set "Continuous Deployment" to On so than any container uploaded replaces the running one.

## Test the Azure Docker Image

After the docker image is created, go to the resource and make note of the URL which will be something like: https:[foo].azurewebsites.net
    
POST: [URL from last Section]/process

Header: 
<code>Content-Type: application/json</code>

Body (raw): 

```json
{
   "values": [
        {
        	"recordId": "a5",
        	"data":
	        {
        	   "metadata_storage_path": "a125",
	           "text":  "The quick brown fox jumped over the lazy dog."
	        }
        }
   ]
}
```

You should get the same response you did when running it locally:

```json
{
    "values": [
        {
            "data": {
                "keyphrases": [
                    "lazy dog",
                    "quick brown fox"
                ]
            },
            "recordId": "a5"
        }
    ]
}
```

## Creating the Azure Search Index

This skillset returns an array of phrases, so we will need to make sure the Azure Search index has a field called phrases.  Here is an example of the schema I used:

```json
	{
    "name": "kpe",
    "defaultScoringProfile": "",
    "fields": [
        {
            "name": "content",
            "type": "Edm.String",
            "searchable": true,
            "filterable": false,
            "retrievable": true,
            "sortable": false,
            "facetable": false,
            "key": false,
            "indexAnalyzer": null,
            "searchAnalyzer": null,
            "analyzer": "en.microsoft",
            "synonymMaps": []
        },
        {
            "name": "metadata_storage_content_type",
            "type": "Edm.String",
            "searchable": false,
            "filterable": true,
            "retrievable": true,
            "sortable": true,
            "facetable": true,
            "key": false,
            "indexAnalyzer": null,
            "searchAnalyzer": null,
            "analyzer": null,
            "synonymMaps": []
        },
        {
            "name": "metadata_storage_size",
            "type": "Edm.Int64",
            "searchable": false,
            "filterable": true,
            "retrievable": true,
            "sortable": true,
            "facetable": true,
            "key": false,
            "indexAnalyzer": null,
            "searchAnalyzer": null,
            "analyzer": null,
            "synonymMaps": []
        },
        {
            "name": "metadata_storage_last_modified",
            "type": "Edm.DateTimeOffset",
            "searchable": false,
            "filterable": true,
            "retrievable": true,
            "sortable": true,
            "facetable": true,
            "key": false,
            "indexAnalyzer": null,
            "searchAnalyzer": null,
            "analyzer": null,
            "synonymMaps": []
        },
        {
            "name": "metadata_storage_content_md5",
            "type": "Edm.String",
            "searchable": false,
            "filterable": true,
            "retrievable": true,
            "sortable": true,
            "facetable": true,
            "key": false,
            "indexAnalyzer": null,
            "searchAnalyzer": null,
            "analyzer": null,
            "synonymMaps": []
        },
        {
            "name": "metadata_storage_name",
            "type": "Edm.String",
            "searchable": false,
            "filterable": true,
            "retrievable": true,
            "sortable": true,
            "facetable": true,
            "key": false,
            "indexAnalyzer": null,
            "searchAnalyzer": null,
            "analyzer": null,
            "synonymMaps": []
        },
        {
            "name": "metadata_storage_path",
            "type": "Edm.String",
            "searchable": false,
            "filterable": true,
            "retrievable": true,
            "sortable": true,
            "facetable": true,
            "key": true,
            "indexAnalyzer": null,
            "searchAnalyzer": null,
            "analyzer": null,
            "synonymMaps": []
        },
        {
            "name": "metadata_content_encoding",
            "type": "Edm.String",
            "searchable": false,
            "filterable": false,
            "retrievable": true,
            "sortable": false,
            "facetable": false,
            "key": false,
            "indexAnalyzer": null,
            "searchAnalyzer": null,
            "analyzer": null,
            "synonymMaps": []
        },
        {
            "name": "metadata_content_type",
            "type": "Edm.String",
            "searchable": false,
            "filterable": false,
            "retrievable": true,
            "sortable": false,
            "facetable": false,
            "key": false,
            "indexAnalyzer": null,
            "searchAnalyzer": null,
            "analyzer": null,
            "synonymMaps": []
        },
        {
            "name": "metadata_language",
            "type": "Edm.String",
            "searchable": false,
            "filterable": false,
            "retrievable": true,
            "sortable": false,
            "facetable": false,
            "key": false,
            "indexAnalyzer": null,
            "searchAnalyzer": null,
            "analyzer": null,
            "synonymMaps": []
        },
        {
            "name": "phrases",
            "type": "Collection(Edm.String)",
            "searchable": true,
            "filterable": true,
            "retrievable": true,
            "sortable": false,
            "facetable": true,
            "key": false,
            "indexAnalyzer": null,
            "searchAnalyzer": null,
            "analyzer": "en.microsoft",
            "synonymMaps": []
        }
    ],
    "scoringProfiles": [],
    "corsOptions": null,
    "suggesters": [
        {
            "name": "sg",
            "searchMode": "analyzingInfixMatching",
            "sourceFields": [
                "keyphrases"
            ]
        }
    ],
    "analyzers": [],
    "tokenizers": [],
    "tokenFilters": [],
    "charFilters": [],
    "encryptionKey": null
}
```

You will also need an Azure Search Datasource to point to your files.  I was using a set of text files and my data source looked like this.  Make sure to update the connection string to your storage account.

```json
{
    "name": "kpe",
    "description": null,
    "type": "azureblob",
    "subtype": null,
    "credentials": {
        "connectionString": "DefaultEndpointsProtocol=https;AccountName=XXX;AccountKey=XXX==;EndpointSuffix=core.windows.net"
    },
    "container": {
        "name": "myfiles",
        "query": ""
    },
    "dataChangeDetectionPolicy": null,
    "dataDeletionDetectionPolicy": null
}

```

The skillset I created looked as follows.  Make sure to update the uri and the cognitiveServices values:
```json
{
    "name": "kpe-skill",
    "description": "basic kpe skillset",
    "skills": [
		{
			"@odata.type": "#Microsoft.Skills.Custom.WebApiSkill",
			"description": "Docker skill",
			"uri": "[URL of your Azure Web App - don't forget to add /process",
			"batchSize":1,
			"context": "/document",
			"inputs": [
			  {
				"name": "text",
				"source": "/document/content",
                "inputs": []
			  },
			  {
				"name": "metadata_storage_path",
				"source": "/document/metadata_storage_path",
                "inputs": []
			  }
			],
			"outputs": [
			  {
				"name": "keyphrases",
				"targetName": "phrases"
			  }
			]
		}
    ],
    
    "cognitiveServices": {
        "@odata.type": "#Microsoft.Azure.Search.CognitiveServicesByKey",
        "description": "/subscriptions/XXXXXX",
        "key": "XXX"
    }

}
```

Finally, you will need an Indexer that will look like:

```json
{
    "name": "kpe",
    "description": null,
    "dataSourceName": "kpe",
    "skillsetName": "kpe-skill",
    "targetIndexName": "kpe",
    "disabled": null,
    "schedule": null,
    "parameters": {
        "batchSize": null,
        "maxFailedItems": 100,
        "maxFailedItemsPerBatch": 100,
        "base64EncodeKeys": false,
        "configuration": {
            "dataToExtract": "contentAndMetadata",
            "parsingMode": "text"
        }
    },
    "fieldMappings": [
        {
            "sourceFieldName": "metadata_storage_path",
            "targetFieldName": "metadata_storage_path",
            "mappingFunction": {
                "name": "base64Encode",
                "parameters": null
            }
        }
    ],
    "outputFieldMappings": [
        {
            "sourceFieldName": "/document/phrases",
            "targetFieldName": "phrases",
            "mappingFunction": null
        }
    ]
}
```
