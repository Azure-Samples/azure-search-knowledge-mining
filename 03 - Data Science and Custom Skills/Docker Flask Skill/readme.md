# UNDER CONSTRUCTION - Do not use yet

# Custom Skill Built using Python and Flask 

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

## Build the Docker Container

From your desktop open "Windows Azure Command Prompt" and go to the directory you downloaded the Github files, of which you should have: 
* ws.py: Hosting the python code which is run as a Flask service.  
* requirements.txt: Includes all the python packages used
* DockerFile: Details on how to build the docker image

Type: docker.exe build -t kpe .

Note, this will take a while as there is a lot to download

## Run the Docker Container Locally

Next, we will test the image locally to make sure it works as expected.  Assuming you have nothing else running on port 8080, you can start the container:

Type: docker.exe run -p 8080:80 kpe

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

## Upload Docker Container to Azure 

We will be uploading the docker image to the Azure Container Registry.  In a subsequent step we will be deploying the docker container to an Azure Web App, however this registry has the nice aspect that it allows us to do contiunous deployment so whenever we add an updated container to the registry it will automatically update the running container.

* [Create an Azure Container registry ](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-get-started-portal)
* Once the registry is created, open it "Access Keys" in the Settings and make note of the Login Server, Username and one of the passwords
* From your desktop in the Azure Command Prompt login to your Azure Subscription by typing: <code>az login</code>
* Log in to your Azure container registry by typing: <code>docker login [Login Server]</code> where [Login Server] is the value taken from the above step

