# Data Science & Custom Skills
AI Search allows you to extend the out of the box functionality with custom skills. This extensibility enables a broad range of features such as creating custom filters, classifying documents, extracting custom entities, and more.

This folder contains some example templates you can leverage to build your own custom skills and some useful Python notebooks you can use if you are building ML models.

AI Search is agnostic to the tool(s) you use to build your custom skills, which are deployed as restful APIs.  The only requirements for building your custom skill(s) are:

1. Have a secure, HTTPS, end-point.
2. Follow the defined input/output schema shown [here](https://docs.microsoft.com/azure/search/cognitive-search-custom-skill-interface). 

The code found in this repo will help you build and deploy these restful APIs in the correct format whether you are building a simple Azure Function or building your own custom ML model.

## Prerequisites
1. If you are using Azure Machine Learning to build custom skills for your solution, you will need to deploy additional resources to Azure subscription.  Please see the [AML Quickstart](https://docs.microsoft.com/azure/machine-learning/service/quickstart-get-started) for more information.

## Types of Custom Skills
In general, there are two types of custom skills that are most commonly used:

### 1.0 Azure Function Custom Skills
Deploying an Azure Function is the quickest and easiest way to create a custom skill. This repo used to have it's own Azure Function templates but now links out to the [PowerSkills repo](https://github.com/Azure-Samples/azure-search-power-skills) that includes an array of custom skill options and fully automates the deployment process. Azure Functions are the recommended approach for deploying skills that do not require an ML model.

### 2.0 Azure Machine Learning Custom Skills
ML models can be used to enhance the AI Search pipeline. In this template, Azure Machine Learning is used to build and deploy the model. The AML Custom Skill Template provides the files needed to quickly deploy a model to be used as a custom skill. 

> This template does not currently use the [AML Custom skill format](https://docs.microsoft.com/azure/search/cognitive-search-aml-skill) that's available in public preview but rather is designed to be deployed as a conventional [Web API skill](https://docs.microsoft.com/azure/search/cognitive-search-custom-skill-web-api). There's a separate tutorial that walks through the process of [creating an AML skill](https://docs.microsoft.com/azure/search/cognitive-search-tutorial-aml-custom-skill) based off of that functionality.

## Useful Links
See the product documentation for more information on custom skills:
1. [Define Custom Skill interface](https://docs.microsoft.com/en-us/azure/search/cognitive-search-custom-skill-interface
)
2. [Custom Skill example](https://docs.microsoft.com/en-us/azure/search/cognitive-search-create-custom-skill-example
)
3. [AML Custom skills](https://docs.microsoft.com/azure/search/cognitive-search-aml-skill)
4. [AML Custom skill example](https://docs.microsoft.com/azure/search/cognitive-search-tutorial-aml-custom-skill)

