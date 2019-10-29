# Installing Insight and Discovery Accelerator

## Prerequisites
1. An Azure Subscription you can access and deploy resources to.
2. Basic familiarity with using the Azure Portal.
3. You will need the .net core SDK installed on the machine running the install. (https://dotnet.microsoft.com/download)

## New Install

1. Pull down or clone the installer folder of the repository.
2. In command prompt or terminal, navigate to the install folder and execute the following command: 

    ```cmd
    dotnet run DeployAzureResources
    ```

3. The first step will open a browser window to a Custom Deployment blade in the Azure Portal.
   
   1. Verify the subscription is correct  
   2. Create a new resource group   
   3. Define a prefix to apply to each of the Azure Resources
      * The prefix should be no more than 7 characters long
      * Do not use spaces or any punctuation including dashes, commas, and underscores   
   
   4. Set the SQL admin username 
   5. Create a SQL admin password   
      * The password must be strong enough to pass the SQL password requirements
   6. Accept the charges.
   7. Deploy
 
3. After the deployment finishes, you will want to copy down the output values to a document so you can use them later in the process. After you have noted the values, press any key in the console to continue to the next step. 

4. The next step will open another browser window to the API Connections blade in the Azure Portal.

   1. The following API Connections must be authorized before continuing:
      * o365_connector_xxxx
      * eventgrid_connector_xxxx

5. After authorizing the API Connections, press any key in the console to continue to the next step

6. The next step will open another browser window to a Custom Deployment blade in the Azure Portal.

   1. Verify the subscription is the same as step 2a

   2. Set the resource group to the resource group created in step 2b

   3. From the output of the deployment from step 2, copy the values into the corresponding parameters. The names will match.

7. After the last deployment finishes, copy the output from the first deployment into the appsettings.json and azuresettings.json files respectively. These files can be found in the install folder. It is important to get these values right because it will drive the rest of the install.
 
9. Once the settings for the installer have been set, run the following command:

    ```cmd
    dotnet run NewInstall
    ```

10. The NewInstall process will run through a series of steps including:

    1. Deploying the Azure Search Resources
    2. Deploying the SQL Database
    3. Seeding the SQL Database
    4. Update App Configurations
    5. Deploying the API
    6. Deploying the UI
    7. Setup blob storage
    8. Invite administrator

11. The authorization must be setup before the administrator can register using the invitation that was sent during the install. This system uses authentication and authorization in Azure App Service. We support all providers supported by this service. Follow this link to add Azure Active Directory authentication. (https://docs.microsoft.com/en-us/azure/app-service/configure-authentication-provider-aad) There are similar guides for each of the authentication providers.
