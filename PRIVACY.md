# Privacy

When you deploy this template, Microsoft is able to identify the installation of the software with the Azure resources that are deployed. Microsoft is able to correlate the Azure resources that are used to support the software. Microsoft collects this information to provide the best experiences with their products and to operate their business. The data is collected and governed by Microsoft's privacy policies, which can be found at https://www.microsoft.com/trustcenter. 

To disable this, simply remove the following section from [azuredeploy.json](./azuredeploy.json) before deploying the resources to Azure:

```json
{ 
            "apiVersion": "2018-02-01",
            "name": "pid-205d0e47-324f-5b9e-8dde-f9fc4a92c091",
            "type": "Microsoft.Resources/deployments",
            "properties": {
                "mode": "Incremental",
                "template": {
                    "$schema": "https://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#",
                    "contentVersion": "1.0.0.0",
                    "resources": []
                }
            }
        }
```

You can see more information on this at https://docs.microsoft.com/en-us/azure/marketplace/azure-partner-customer-usage-attribution.