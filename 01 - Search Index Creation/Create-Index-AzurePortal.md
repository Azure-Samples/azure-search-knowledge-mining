# Creating a Search Index in the Azure Portal

The steps below will walk you through creating a search index in the Azure Portal using the [import data wizard](https://docs.microsoft.com/en-us/azure/search/search-import-data-portal). For more detailed information on each of the steps, see the the [Blob Storage Quickstart](https://docs.microsoft.com/en-us/azure/search/cognitive-search-quickstart-blob).

The web app you'll spin up in the next step expects to receive several fields to work properly. Because of this, be sure to follow the requirements outlined below to avoid any problems during setup.

## Requirements

If you choose to create your index via the Azure Portal, set your search field properties as described in the table below:

| Field					| Notes and Expectations						|
|-----------------------|-----------------------------------------------|
|content				| Used to show the transcript of the files.  Should be **searchable and retrievable**  |
|metadata_storage_path	| This should be the **key field**. 	 The storage path is used to query the blob indexer for the content so that you can "preview" the file.  Should be **retrievable**.	 If it is **base64 encoded**, make sure to specify it in the [appsettings.json](https://github.com/Azure-Samples/azure-search-knowledge-mining/tree/master/02%20-%20Web%20UI%20Template) file of the front end application.		|
|metadata_storage_name	| The storage name is used to display the name of the file on the results page.  Should be **retrievable**.	|
|people					| List of strings with the persons identified in the document.  Should be **facetable, filterable, searchable and retrievable**.  |
|locations				| List of strings with the locations identified in the document. Should be **facetable, filterable, searchable and retrievable**.  |
|organizations			| List of strings with the organizations identified in the document. Should be **facetable, filterable, searchable and retrievable**.  |
|keyPhrases				| List of strings with the key phrases identified in the document. Should be **facetable, filterable, searchable and retrievable**.  |
|language				| String containing the main language of the document. Should be **facetable, filterable, searchable and retrievable**.  |

## Instructions

Follow the steps and screenshots below to create your index.

### 1.0 Navigate to you Search Service

Start by navigating to your search service in the Azure Portal:

![screenshot](../images/createindex-step0.PNG)

### 2.0 Select Import data

![Navigate to search service](../images/createindex-step1.PNG)

### 3.0 Import Data

#### 3.1 Select Azure Blob Storage

![screenshot](../images/createindex-step2.PNG)

#### 3.2 Follow the wizard to connect to your storage account

Keep the defaults and use `Choose an existing connection` to connect to your storage container with your data:

![screenshot](../images/createindex-step3.PNG)

### 4.0 Add Cognitive Skills

#### 4.1 Attach Cognitive Services

An Cognitive Services reosurce should have been automatically provisioned, just select it. If it has not been provisioned successfully, use the `Create new Cognitive Services resource` link and create one:

![screenshot](../images/createindex-step4.PNG)

#### 4.2 Add enrichments

Select the enrichments you want to add. Please note that the `Extract personally identifiable information` skill currently does only support English documents. Some of the sample data is currently in different languages, hence do not enable it when you use the [sample documents](../sample_documents) from this repo. 

![screenshot](../images/createindex-step5.PNG)

#### 4.3 Save enrichments to knowledge store

![screenshot](../images/createindex-step6.PNG)

### 5.0 Customize target index

> **Note:** This step is essential to properly configuring your index. Make sure your index looks similar to the screenshot before proceeding to the next step.

![screenshot](../images/createindex-step7.PNG)

### 6.0 Create an indexer

![screenshot](../images/createindex-step8.PNG)

Congratulations! You should be all set to move onto the next folder.
