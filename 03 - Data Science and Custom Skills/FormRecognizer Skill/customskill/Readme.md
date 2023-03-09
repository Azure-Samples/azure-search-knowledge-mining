
# Form Recognizer Custom Skill

Follow MS Learn module [Build a Form Recognizer custom skill for Azure Cognitive Search ](https://learn.microsoft.com/en-us/training/modules/build-form-recognizer-custom-skill-for-azure-cognitive-search/4-exercise-build-deploy)

Invoking a Form Recognizer capability within the Cognitive Search pipeline is broken out into three skills
* [Analyze Form](#AnalyzeForm), using a pre trained model
* [Analyze Invoice](#AnalyzeInvoice) using a pretrained model
* [Extract Tables](#ExtractTables) using a pretrained model

# Deployment    

The analyze form skill requires you to train a custom model to identify and extract key value pairs. Analyze invoice and extract table skills invoke pre trained models. All skills require the `FORMS_RECOGNIZER_ENDPOINT` and `FORMS_RECOGNIZER_KEY` property set in the appsettings, while the analyze form skill additionally requires a `FORMS_RECOGNIZER_MODEL_ID`.

To deploy the skills:
1. In the Azure portal, create a Forms Recognizer resource.
2. Copy the form recognizer URL and key for use in the training and appsettings.
3. Clone this repository
4. Upload the training data and test data to an Azure storage account and generate a SAS token
4. Use the training notebook to train a model (using either the provided training data or your fomrs)
5. Open the AnalyzeFormV2 folder in VS Code and deploy the function.
6. Once the function is deployed, set the required appsettings (`FORMS_RECOGNIZER_ENDPOINT`, `FORMS_RECOGNIZER_KEY` and `FORMS_RECOGNIZER_MODEL_ID`)

# AnalyzeForm

This custom skill extracts specific fields from the results of a trained form recognition.

A [full tutorial on this skill is available in the Azure Cognitive Seach documentation](https://docs.microsoft.com/en-us/azure/search/cognitive-search-custom-skill-form).


## Requirements

In addition to the common requirements described in [the root `README.md` file](../../README.md), this function requires access to an [Azure Form Recognizer](https://azure.microsoft.com/en-us/services/cognitive-services/form-recognizer/) resource. 

[Train a model with your forms](https://docs.microsoft.com/azure/cognitive-services/form-recognizer/quickstarts/client-library) before you can use this skill. As a sample to get started, use the included [sample training forms](Train) and [sample test form](Test) with the [training notebook](FormRecognizerTrainModel.ipynb) to create a model in just a few minutes.

## Settings

This function requires a `FORMS_RECOGNIZER_ENDPOINT` and a `FORMS_RECOGNIZER_KEY` settings set to a valid Azure Forms Recognizer API key and to your custom Form Recognizer 2.1-preview endpoint. 
If running locally, this can be set in your project's local environment variables. This ensures your key won't be accidentally checked in with your code.
If running in an Azure function, this can be set in the application settings.

After training, you will need to set the `FORMS_RECOGNIZER_MODEL_ID` application setting to the model id corresponding to your trained model.

The list of fields to extract and the fields they get mapped to in the response of the skill need to be configured to reflect your particular scenario. This can be done by editing the [`field-mappings.json`] file(https://github.com/Azure-Samples/azure-search-power-skills/blob/master/Vision/AnalyzeFormV2/AnalyzeForm/field-mappings.json).

## Sample Input:

This sample data is pointing to a file stored in this repository, but when the skill is integrated in a skillset, the URL and token will be provided by cognitive search.

```json
{
    "values": [
        {
            "recordId": "record1",
            "data": { 
                "formUrl": "https://github.com/Azure-Samples/azure-search-power-skills/raw/master/SampleData/Invoice_4.pdf",
                "formSasToken":  "?st=sasTokenThatWillBeGeneratedByCognitiveSearch"
            }
        }
    ]
}
```

## Sample Output:

```json
{
    "values": [
        {
            "recordId": "record1",
            "data": {
                "address": "1111 8th st. Bellevue, WA 99501 ",
                "recipient": "Southridge Video 1060 Main St. Atlanta, GA 65024 "
            },
            "errors": null,
            "warnings": null
        }
    ]
}
```

## Sample Skillset Integration

In order to use this skill in a cognitive search pipeline, you'll need to add a skill definition to your skillset.
Here's a sample skill definition for this example (inputs and outputs should be updated to reflect your particular scenario and skillset environment):

```json
{
    "@odata.type": "#Microsoft.Skills.Custom.WebApiSkill",
    "name": "formrecognizer", 
    "description": "Extracts fields from a form using a pre-trained form recognition model",
    "uri": "[AzureFunctionEndpointUrl]/api/AnalyzeForm?code=[AzureFunctionDefaultHostKey]",
    "httpMethod": "POST",
    "timeout": "PT1M",
    "context": "/document",
    "batchSize": 1,
    "inputs": [
        {
            "name": "formUrl",
            "source": "/document/metadata_storage_path"
        },
        {
            "name": "formSasToken",
            "source": "/document/metadata_storage_sas_token"
        }
    ],
    "outputs": [
        {
            "name": "address",
            "targetName": "address"
        },
        {
            "name": "recipient",
            "targetName": "recipient"
        }
    ]
}
```

# AnalyzeInvoice

This custom skill extracts invoice specific fields using a pre trained forms recognizer model.


## Requirements

In addition to the common requirements described in [the root `README.md` file](../../README.md), this function requires access to an [Azure Forms Recognizer](https://azure.microsoft.com/en-us/services/cognitive-services/form-recognizer/) resource. The [prebuilt invoice model](https://docs.microsoft.com/azure/cognitive-services/form-recognizer/concept-invoices) is available in the 2.1 preview API.

## Settings

This function requires a `FORMS_RECOGNIZER_ENDPOINT` and a `FORMS_RECOGNIZER_KEY` settings set to a valid Azure Forms Recognizer API key and to your custom Form Recognizer 2.1-preview endpoint. 
If running locally, this can be set in your project's local environment variables. This ensures your key won't be accidentally checked in with your code.
If running in an Azure function, this can be set in the application settings.


## Sample Input:

This sample data is pointing to a file stored in this repository, but when the skill is integrated in a skillset, the URL and token will be provided by cognitive search.

```json
{
    "values": [
        {
            "recordId": "record1",
            "data": { 
                "formUrl": "https://github.com/Azure-Samples/azure-search-power-skills/raw/master/SampleData/Invoice_4.pdf",
                "formSasToken":  "?st=sasTokenThatWillBeGeneratedByCognitiveSearch"
            }
        }
    ]
}
```

## Sample Output:

```json
{
    "values": [
        {
            "recordId": "0",
            "data": {
                "invoices": [
                    {
                        "AmountDue": 63.0,
                        "BillingAddress": "345 North St NY 98052",
                        "BillingAddressRecipient": "Fabrikam, Inc.",
                        "DueDate": "2018-05-31",
                        "InvoiceDate": "2018-05-15",
                        "InvoiceId": "1785443",
                        "InvoiceTotal": 56.28,
                        "VendorAddress": "4567 Main St Buffalo NY 90852",
                        "SubTotal": 49.3,
                        "TotalTax": 0.99
                    }
                ]
            }
        }
    ]
}
```

## Sample Skillset Integration

In order to use this skill in a cognitive search pipeline, you'll need to add a skill definition to your skillset.
Here's a sample skill definition for this example (inputs and outputs should be updated to reflect your particular scenario and skillset environment):

```json
{
    "@odata.type": "#Microsoft.Skills.Custom.WebApiSkill",
    "name": "formrecognizer", 
    "description": "Extracts fields from a form using a pre-trained form recognition model",
    "uri": "[AzureFunctionEndpointUrl]/api/AnalyzeInvoice?code=[AzureFunctionDefaultHostKey]",
    "httpMethod": "POST",
    "timeout": "PT1M",
    "context": "/document",
    "batchSize": 1,
    "inputs": [
        {
            "name": "formUrl",
            "source": "/document/metadata_storage_path"
        },
        {
            "name": "formSasToken",
            "source": "/document/metadata_storage_sas_token"
        }
    ],
    "outputs": [
        {
            "name": "invoices",
            "targetName": "invoices"
        }
    ]
}
```

# ExtractTables

This custom skill extracts tables using a pre trained forms recognizer model.


## Requirements

In addition to the common requirements described in [the root `README.md` file](../../README.md), this function requires access to an [Azure Forms Recognizer](https://azure.microsoft.com/en-us/services/cognitive-services/form-recognizer/) resource. The [layout service](https://docs.microsoft.com/azure/cognitive-services/form-recognizer/concept-layout) is available in the 2.1 preview API.

## Settings

This function requires a `FORMS_RECOGNIZER_ENDPOINT` and a `FORMS_RECOGNIZER_KEY` settings set to a valid Azure Forms Recognizer API key and to your custom Form Recognizer 2.1-preview endpoint. 
If running locally, this can be set in your project's local environment variables. This ensures your key won't be accidentally checked in with your code.
If running in an Azure function, this can be set in the application settings.


## Sample Input:

This sample data is pointing to a file stored in this repository, but when the skill is integrated in a skillset, the URL and token will be provided by cognitive search.

```json
{
    "values": [
        {
            "recordId": "record1",
            "data": { 
                "formUrl": "https://github.com/Azure-Samples/azure-search-power-skills/raw/master/SampleData/Invoice_4.pdf",
                "formSasToken":  "?st=sasTokenThatWillBeGeneratedByCognitiveSearch"
            }
        }
    ]
}
```

## Sample Output:

```json
{
    "values": [
        {
            "recordId": "0",
            "data": {
                "tables": [
                    {
                        "page_number": 1,
                        "row_count": 4,
                        "column_count": 4,
                        "cells": [
                            {
                                "text": "Item",
                                "rowIndex": 0,
                                "colIndex": 0,
                                "confidence": 1.0,
                                "is_header": false
                            },
                            {
                                "text": "Quantity",
                                "rowIndex": 0,
                                "colIndex": 1,
                                "confidence": 1.0,
                                "is_header": false
                            },
                            {
                                "text": "Rate",
                                "rowIndex": 0,
                                "colIndex": 2,
                                "confidence": 1.0,
                                "is_header": false
                            },
                            {
                                "text": "Amount",
                                "rowIndex": 0,
                                "colIndex": 3,
                                "confidence": 1.0,
                                "is_header": false
                            },
                            {
                                "text": "Monthly service fee",
                                "rowIndex": 1,
                                "colIndex": 0,
                                "confidence": 1.0,
                                "is_header": false
                            },
                            {
                                "text": "01",
                                "rowIndex": 1,
                                "colIndex": 1,
                                "confidence": 1.0,
                                "is_header": false
                            },
                            {
                                "text": "$40.00",
                                "rowIndex": 1,
                                "colIndex": 2,
                                "confidence": 1.0,
                                "is_header": false
                            },
                            {
                                "text": "$40.00",
                                "rowIndex": 1,
                                "colIndex": 3,
                                "confidence": 1.0,
                                "is_header": false
                            },
                            {
                                "text": "Guarantee monthly fee",
                                "rowIndex": 2,
                                "colIndex": 0,
                                "confidence": 1.0,
                                "is_header": false
                            },
                            {
                                "text": "01",
                                "rowIndex": 2,
                                "colIndex": 1,
                                "confidence": 1.0,
                                "is_header": false
                            },
                            {
                                "text": "$6.00",
                                "rowIndex": 2,
                                "colIndex": 2,
                                "confidence": 1.0,
                                "is_header": false
                            },
                            {
                                "text": "$6.00",
                                "rowIndex": 2,
                                "colIndex": 3,
                                "confidence": 1.0,
                                "is_header": false
                            },
                            {
                                "text": "Add-on services",
                                "rowIndex": 3,
                                "colIndex": 0,
                                "confidence": 1.0,
                                "is_header": false
                            },
                            {
                                "text": "01",
                                "rowIndex": 3,
                                "colIndex": 1,
                                "confidence": 1.0,
                                "is_header": false
                            },
                            {
                                "text": "$3.30",
                                "rowIndex": 3,
                                "colIndex": 2,
                                "confidence": 1.0,
                                "is_header": false
                            },
                            {
                                "text": "$3.30",
                                "rowIndex": 3,
                                "colIndex": 3,
                                "confidence": 1.0,
                                "is_header": false
                            }
                        ]
                    }
                ]
            }
        }
    ]
}
```

## Sample Skillset Integration

In order to use this skill in a cognitive search pipeline, you'll need to add a skill definition to your skillset.
Here's a sample skill definition for this example (inputs and outputs should be updated to reflect your particular scenario and skillset environment):

```json
{
    "@odata.type": "#Microsoft.Skills.Custom.WebApiSkill",
    "name": "formrecognizer", 
    "description": "Extracts fields from a form using a pre-trained form recognition model",
    "uri": "[AzureFunctionEndpointUrl]/api/ExtractTables?code=[AzureFunctionDefaultHostKey]",
    "httpMethod": "POST",
    "timeout": "PT1M",
    "context": "/document",
    "batchSize": 1,
    "inputs": [
        {
            "name": "formUrl",
            "source": "/document/metadata_storage_path"
        },
        {
            "name": "formSasToken",
            "source": "/document/metadata_storage_sas_token"
        }
    ],
    "outputs": [
        {
            "name": "tables",
            "targetName": "tables"
        }
    ]
}
```
