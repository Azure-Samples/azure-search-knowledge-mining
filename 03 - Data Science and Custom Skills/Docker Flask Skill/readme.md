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
* ws.py: Hosting the python code which is run as a Flask service
* requirements.txt: Includes all the python packages used
* DockerFile: Details on how to build the docker image

Type: docker.exe build -t kpe .

At this point, the Docker image will be built and stored in your local Docker Desktop repository.  
