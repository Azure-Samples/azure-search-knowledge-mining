# Securing Secrets in Web App using Azure Key Vault

This section outlines how to leverage Azure Key Vault to secure secrets such as the Azure Search and Azure Blob API-Keys in a safe location as opposed to storing the secrets in the code of the Web App.

## Create the Key Vault
From Azure Portal, create a new “Key Vault”
![](/images/create-kv-1.png)
![](/images/create-kv-2.png)

Once the Key Vault resource has been created, choose "Secrets" from the menu.  This is where we will store the api-key of the Azure Search service.  From this page choose "Generate/Import"

![](/images/kv-create-secret.png)

Set the following values:
* Name: azure-search-api-key
* Value: [Enter your Azure Search Admin API Key]

NOTE: To get your Azure Search API Key, open your Azure Search service and choose "Keys".  You can use your Admin API key for this demo, however, it is best practice to use least privileges, so you may want to create and use a [Query API Key](https://docs.microsoft.com/en-us/azure/search/search-security-api-keys).

![](/images/kv-set-secret.png)

Choose "Create" to add this key to the key vault.

Once again, choose "Generate/Import" to create a secret to store the Azure Blob API Key.  

Set the following values:
* Name: azure-blob-api-key
* Value: [Enter your Azure Blob API Key]

![](/images/kv-set-secret-blob.png)

Choose "Create" to add this key to the key vault and you should see two secrets as follows:

![](/images/kv-view-list.png)

## Configure Web App to use Key Vault

This section assumes you have at least completed Module 2 which created an Web App to search and visualize data from Azure Search.  In this section, we will remove the "SearchApiKey" and "StorageAccountKey" stored in the appsettings.json file and update the code to load the Azure Search API and Azure Blob API key from Key Vault when the application starts.

### Add Nuget Package

From Visual Studio, open the CognitiveSearch.UI project and from the Solution Explorer, right click on Dependancies and choose "Manage Nuget Packages".  Search for and add:
```
Microsoft.Extensions.Configuration.AzureKeyVault
```
At the time of writing, version 3.0.0 was used.

![](/images/kv-nuget.png)

### Remove Secrets

From Visual Studio, open the CognitiveSearch.UI project and open appsettings.json.

Remove the lines:
```
"SearchApiKey": ...
"StorageAccountKey": ...
```

### Add Code to Download Secrets from Key Vault

Open Program.cs and update the BuildWebHost to load the Key Vault secrets as follows.  Update [Your Key Vault Service Name] to the name of your Key Vault service name.  

```
        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((context, config) =>
                {
                    // You can alter below code between dev and production using context.HostingEnvironment.IsProduction()
                    var builtConfig = config.Build();

                    var azureServiceTokenProvider = new AzureServiceTokenProvider();
                    var keyVaultClient = new KeyVaultClient(
                        new KeyVaultClient.AuthenticationCallback(
                            azureServiceTokenProvider.KeyVaultTokenCallback));

                    config.AddAzureKeyVault(
                        $"https://[Your Key Vault Service Name].vault.azure.net/",
                        keyVaultClient,
                        new DefaultKeyVaultSecretManager());
                })
                .Build();

```

At the top of Program.cs, add the following references:

```
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Logging;
```

This code will run when the web app first runs and load all the secrets into the config for this web app which are then accessible by the application as needed.  One of the great capabilities of Key Vault is the fact that when you run it locally, it will use the user you are configured to log in as.  If you need to change or view the user currently configured, choose Tools -> Options -> Azure Service Authentication -> Account Selection.  In order for the above code to be able to access your key vault when running from your local machine, this user should match what is configured in your Key Vault under "Access policies".  You can choose to remove or adjust the access of this user as needed.

![](/images/kv-access-policies.png)

Finally, we can update the calls to set the Azure Search and Azure Blob API keys with the values loaded from Key Vault.  To do this, open DocumentSearchClient.cs and replace the line:

```
apiKey = configuration.GetSection("SearchApiKey")?.Value;
```

with the Key Vault secret named azure-search-api-key:
```
apiKey = configuration.GetSection("azure-search-api-key")?.Value;
```

Open HomeController.cs and update the Blob API Key from:

```
string accountKey = _configuration.GetSection("StorageAccountKey")?.Value;
```

to:

```
string accountKey = _configuration.GetSection("azure-blob-api-key")?.Value;
```

You may wish to set breakpoints on the code that you added to make sure things are set as you expected.

At this point, you should be able to build and run the project.

### Deploying the Web App (Azure App Services)

When deploying the web app to Azure App Services, we will want to have our Key Vault trust the app service hosting this web app.  To do this, you will first need to have an Azure App Service created.  Within the App Service choose Identity and under "System Assigned", turn it ON and Save the changes.  Copy the Object ID created.  This creates what is called a System Managed Identity.  This managed identity is registered in Azure Active Directory and can then be used by Key Vault to authenticate the app service and allow it to access the secrets.

![](/images/kv-app-identity.png)

Now, switch to your Azure Key Vault and choose Access Policies and choose "Add Access Policy".  

* Secret Permissions: Choose Get & List
* Select Principal: Click and paste in the Object ID copied from the previous step where you created the managed identity.  This should find your web app.  You can alternatively choose the name of your web app, however make sure you choose the correct one.

![](/images/kv-access-policy-system.png)

Add this policy and choose Save.

You can now deploy your app from Visual Studio to this app service and everything should work, just as it did when running locally.

### Final Thoughts

The above steps are very comprehensive for securing credentials for the purposes of testing and proof of concepts.  As you progress in development, it is very likely you will leverage techniques such as Continuous Integration.  This typically will need to consider additional aspects such as separation of dev and production keys.  It is highly recommended that you [read more about this here](https://docs.microsoft.com/en-us/azure/key-vault/key-vault-best-practices).
