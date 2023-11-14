
# Form Recognizer Custom Skill

Follow MS Learn module [Build a Form Recognizer custom skill for Azure Cognitive Search ](https://learn.microsoft.com/en-us/training/modules/build-form-recognizer-custom-skill-for-azure-cognitive-search/4-exercise-build-deploy)
to create Form Recognizer service and deploy Azure Function using cloud shell.

Integrate a Form Recognizer Pre-Built Model for Invoices capability within the Cognitive Search pipeline

# AnalyzeInvoice

This custom skill extracts invoice specific fields using a pre trained forms recognizer model.


##  Settings

This Azure function requires access to an [Azure Forms Recognizer](https://azure.microsoft.com/en-us/services/cognitive-services/form-recognizer/) resource. The [prebuilt invoice model](https://docs.microsoft.com/azure/cognitive-services/form-recognizer/concept-invoices) is available in the 2.1 preview API.


This function requires a `FORMS_RECOGNIZER_ENDPOINT` and a `FORMS_RECOGNIZER_KEY` settings set to a valid Azure Forms Recognizer API key and to your custom Form Recognizer 2.1-preview endpoint. 



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

Refer to Postman Collection for more details.