# Search Index Creation

There are several different ways to create a search index for Azure Cognitive Search. You can choose how to deploy an index based on your skillset, your preferred tools, or your desired level of automation.

>**Note:** If you ran the `deploy.ps1` script in the previous step, you've already created your search index so you can skip to the next step.

## Prerequisites

At this point, you should have:

1. Deployed the necessary resources to Azure using the ARM template or PowerShell script as described in the previous step.
1. Uploaded your document set to Azure Storage (e.g., a container in Azure Blob Storage). You can also do this later using the Web App.

### Uploading sample data

You can either use your own dataset, or use the [sample documents](../sample_documents/) in this repo. The quickest way to upload the data is to create a new container in the provisioned Storage Account and then use the `Storage Explorer` in the Azure Portal to upload the files.

## Options for creating your index

This folder includes three options for creating an index. Each of these approaches is documented in a separate file:

1. [Create a search index using the Azure Portal](./Create-Index-AzurePortal.md)
2. [Create a search index using PowerShell](./Create-Index-PowerShell.md)
3. [Create a search index using Postman](./Create-Index-Postman.md)