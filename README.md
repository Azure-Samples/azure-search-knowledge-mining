---
page_type: sample
languages:
- csharp
- javascript
- html
- powershell
products:
- azure
description: "Welcome to the Knowledge Mining Solution Accelerator!"
urlFragment: azure-search-knowledge-mining
---

![Knowledge Mining Solution Accelerator](images/kmheader.png)

# Knowledge Mining Solution Accelerator

## About this repository

Welcome to the Knowledge Mining Solution Accelerator! This accelerator provides developers with all of the resources they need to quickly build an initial Knowledge Mining prototype with [Azure Cognitive Search](https://docs.microsoft.com/azure/search/cognitive-search-concept-intro). Use this accelerator to jump-start your development efforts with your own data or as a learning tool to better understand how you can use Cognitive Search to meet the unique needs of your business.

In this repository, we've provided you with all of the artifacts you need to quickly create a Cognitive Search Solution including: templates for deploying the appropriate Azure resources, assets for creating your first search index, templates for using custom skills, a basic web app, and PowerBI reports to monitor search solution performance. We've infused best practices throughout the documentation to help guide you. With Cognitive Search, you can easily index both digital data (such as documents and text files) and analog data (such as images and scanned documents).

> Note: This guide uses the AI enrichment feature of Cognitive Search. AI enrichment allows you to ingest many kinds of data (documents, text files, images, scanned docs, and more), extract their contents, enrich and transform it, and then index it for exploration purposes. To learn more about this feature, see the [AI in Cognitive Search](https://docs.microsoft.com/azure/search/cognitive-search-concept-intro) doc.

Once you're finished, you'll have a web app ready to search your data.

![A web app showing several resources and their lists of searchable tags](images/ui.PNG)

## Prerequisites

In order to successfully complete your solution, you'll need to gain access and provision the following resources:

* Azure subscription - [Create one for free](https://azure.microsoft.com/free/)
* [Visual Studio 2017 or later](https://visualstudio.microsoft.com/downloads/)
* [Postman](https://www.getpostman.com/) for making API calls
* Documents uploaded to any data source supported by Azure Search Indexers. For a list of these, see [Indexers in Azure Cognitive Search](https://docs.microsoft.com/azure/search/search-indexer-overview). This solution accelerator uses Azure Blob Storage as a container for source data files. You can find sample documents in the **sample_documents/** folder.

The directions provided in this guide assume you have a fundamental working knowledge of the Azure portal, Azure Functions, Azure Cognitive Search, Visual Studio and Postman. For additional training and support, please see:

* [Knowledge Mining Bootcamp](https://github.com/Azure/LearnAI-KnowledgeMiningBootcamp)
* [AI in Cognitive Search documentation](https://docs.microsoft.com/azure/search/cognitive-search-resources-documentation)

## Process overview

Clone or download this repository and then navigate through each of these folders in order, following the steps outlined in each of the README files. When you complete all of the steps, you'll have a working end-to-end solution that combines data sources with data enrichment skills, a web app powered by Azure Cognitive Search, and intelligent reporting on user search activity.

![the cognitive indexing pipelines used for processing unstructured data in Azure Search](images/architecture.jpg)

### [00 - Resource Deployment](https://github.com/Azure-Samples/azure-search-knowledge-mining/tree/master/00%20-%20Resource%20Deployment)

The contents of this folder show you how to deploy the required resources to your Azure subscription. You can do this either through the [Azure portal](https://portal.azure.com) or using the provided [PowerShell script](https://github.com/Azure-Samples/azure-search-knowledge-mining/00%20-%20Resource%20Deployment/deploy.ps1).

Alternatively, you can automatically deploy the required resources using this button:

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fazure-search-knowledge-mining%2Fmaster%2Fazuredeploy.json" target="_blank">
    <img src="http://azuredeploy.net/deploybutton.png"/>
</a>

### [01 - Search Index Creation](https://github.com/Azure-Samples/azure-search-knowledge-mining/tree/master/01%20-%20Search%20Index%20Creation)

This folder contains a Postman collection that you can use to create a search index. The collection is pre-configured to take advantage of out-of-the-box Cognitive Search functionality.

We recommend using this collection to create an initial search index and then iterating by editing the postman collection and adding custom skills as needed.

### [02 - Web UI Template](https://github.com/Azure-Samples/azure-search-knowledge-mining/tree/master/02%20-%20Web%20UI%20Template)

This folder contains a basic Web UI Template, written in .NET Core, which you can configure to query your search index. Follow the steps outlined in the [Web UI Template README file](https://github.com/Azure-Samples/azure-search-knowledge-mining/02%20-%20Web%20UI%20Template/README.md) to integrate your new search index into the web app.

### [03 - Data Science & Custom Skills](https://github.com/Azure-Samples/azure-search-knowledge-mining/tree/master/03%20-%20Data%20Science%20and%20Custom%20Skills)

This folder contains examples and templates to add your own custom skills to your solution. These custom skills help to align the solution to the needs of your particular use case. This step is entirely optional and may be skipped if not needed.

For additional samples and information on custom skill development, see the [Custom skill documentation](https://docs.microsoft.com/azure/search/cognitive-search-custom-skill-interface). .NET Azure Function Custom Skills have moved to the [Power Skills repository](https://github.com/Azure-Samples/azure-search-power-skills).

### [04 - Reporting](https://github.com/Azure-Samples/azure-search-knowledge-mining/tree/master/04%20-%20Reporting)

This folder contains pre-built PowerBI reports that you can use to monitor your solution and to understand user search behavior. They leverage data captured through [Application Insights](https://docs.microsoft.com/azure/azure-monitor/app/app-insights-overview) and can be modified to meet your particular business objectives. This step is entirely optional and may be skipped if not needed.

### [Sample Documents](https://github.com/Azure-Samples/azure-search-knowledge-mining/tree/master/sample_documents)

This folder contains a small data set in a variety of file formats that you can use to build your solution if you don't have another data set available.

### [Workshop](https://github.com/Azure-Samples/azure-search-knowledge-mining/tree/master/workshops)

Become an Azure Cognitive Search expert in a day!
This folder contains a self paced workshop that teaches you everything you need to know. Most developer with Azure familiarity should be able to complete the majority of the modules below in 8 hours.

+ [Module 0 - Pre-Requisites](https://github.com/Azure-Samples/azure-search-knowledge-mining/blob/master/workshops/Module%200.md) (*you must complete prior to moving on!*)
+ [Module 1 - Using Azure Portal to Build a Search Index and Knowledge Store](https://github.com/Azure-Samples/azure-search-knowledge-mining/blob/master/workshops/Module%201.md)
+ [Module 2 - Visualizing the Results with a Demo Front-End](https://github.com/Azure-Samples/azure-search-knowledge-mining/blob/master/workshops/Module%202.md)
+ [Module 3 - Introduction to Custom Skills and Azure Functions](https://github.com/Azure-Samples/azure-search-knowledge-mining/blob/master/workshops/Module%203.md)
+ [Module 4 - Learning the Object Model](https://github.com/Azure-Samples/azure-search-knowledge-mining/blob/master/workshops/Module%204.md)
+ [Module 5 - Advanced Azure Cognitive Search: Analyzers and Scoring Profiles](https://github.com/Azure-Samples/azure-search-knowledge-mining/blob/master/workshops/Module%205.md)
+ [Module 6 - Analyzing Your Data with PowerBI](https://github.com/Azure-Samples/azure-search-knowledge-mining/blob/master/workshops/Module%206.md)
+ [Module 7 - Using Azure Cognitive Search to index structured data](https://github.com/Azure-Samples/azure-search-knowledge-mining/blob/master/workshops/Module%207.md) (Optional)

## License

Please refer to [LICENSE](https://github.com/Azure-Samples/azure-search-knowledge-mining/LICENSE.md) for all licensing information.
