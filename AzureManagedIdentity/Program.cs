using System;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Rest;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;

using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.AppService.Fluent.Models;
using System.Reflection;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Graph.RBAC.Fluent;
using System.Collections.Generic;
using System.Linq;

namespace AzureManagedIdentity
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
            await UpdateFunctionAppSettings();
            // GetResourceGroups(azureServiceTokenProvider).Wait();
            // GetAppSettingsOfFunctionApp(azureServiceTokenProvider).Wait();
            if (azureServiceTokenProvider.PrincipalUsed != null)
            {
                Console.WriteLine($"{Environment.NewLine}Principal used: {azureServiceTokenProvider.PrincipalUsed}");
            }

            Console.ReadKey();
        }

        //private static async Task GetResourceGroups(AzureServiceTokenProvider azureServiceTokenProvider)
        //{
        //    var subscriptionId = "56710d55-e631-4017-9ffc-380a1bfb05fe";

        //    try
        //    {
        //        var serviceCreds = new TokenCredentials(await azureServiceTokenProvider.GetAccessTokenAsync("https://management.azure.com/", "4c91007c-ce23-4870-a0bc-3df6aff5f0da").ConfigureAwait(false));

        //        var resourceManagementClient =
        //            new ResourceManagementClient(serviceCreds) { SubscriptionId = subscriptionId };

        //        var resourceGroups = await resourceManagementClient.ResourceGroups.ListAsync();

        //        foreach (var resourceGroup in resourceGroups)
        //        {
        //            Console.WriteLine($"Resource group {resourceGroup.Name}");
        //        }

        //    }
        //    catch (Exception exp)
        //    {
        //        Console.WriteLine($"Something went wrong: {exp.Message}");
        //    }
        //}

        public async Task GetWebAppDetails()
        {
            string clientId = "clientId";
            string clientSecret = "clientSecret";
            string tenantId = "tenantId";
            string subscriptionId = "subscriptionId";

            AzureCredentials credentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(
                clientId,
                clientSecret,
                tenantId,
                AzureEnvironment.AzureGlobalCloud).WithDefaultSubscription(subscriptionId);

            RestClient restClient = RestClient
                .Configure()
                .WithEnvironment(AzureEnvironment.AzureGlobalCloud)
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .WithCredentials(credentials)
                .Build();

            var creds = new AzureCredentialsFactory().FromServicePrincipal(
                clientId,
                clientSecret,
                tenantId,
                AzureEnvironment.AzureGlobalCloud
                );


            var azure = Microsoft.Azure.Management.Fluent.Azure.Authenticate(creds);
            // Get all subscriptions in tenant
            foreach (var subscription in await azure.Subscriptions.ListAsync())
            {
                // Get all resource groups in subscription
                Microsoft.Azure.Management.ResourceManager.Fluent.ResourceManagementClient resourceManagementClient = new Microsoft.Azure.Management.ResourceManager.Fluent.ResourceManagementClient(restClient);
                resourceManagementClient.SubscriptionId = subscription.SubscriptionId;

                foreach (var resourceGroup in await resourceManagementClient.ResourceGroups.ListAsync())
                {
                    // Get web app details
                    WebSiteManagementClient webSiteManagementClient = new WebSiteManagementClient(restClient);
                    webSiteManagementClient.SubscriptionId = subscription.SubscriptionId;

                    foreach (var webApp in await azure.WithSubscription(subscription.SubscriptionId).WebApps.ListByResourceGroupAsync(resourceGroup.Name))
                    {
                        SiteConfigResourceInner siteConfigResourceInner = await webSiteManagementClient.WebApps.GetConfigurationAsync(resourceGroup.Name, webApp.Name);
                        webApp.Inner.SiteConfig = new SiteConfig();

                        foreach (PropertyInfo propertyInfo in webApp.Inner.SiteConfig.GetType().GetProperties())
                        {
                            var value = siteConfigResourceInner.GetType().GetProperty(propertyInfo.Name).GetValue(siteConfigResourceInner, null);
                            propertyInfo.SetValue(webApp.Inner.SiteConfig, siteConfigResourceInner.GetType().GetProperty(propertyInfo.Name).GetValue(siteConfigResourceInner));
                        }
                    }
                }
            }
        }

        private static async Task UpdateFunctionAppSettings()
        {
            try
            {
                string clientId = "clientId";
                string clientSecret = "clientSecret";
                string tenantId = "tenantId";
                string subscriptionId = "subscriptionId";
                string functionName = "functionName";
                string resourceGroupName = "resourceGroupName";
                AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                var serviceCreds = new TokenCredentials(await azureServiceTokenProvider.GetAccessTokenAsync("https://management.azure.com/", tenantId).ConfigureAwait(false));
                var graphCreds = new TokenCredentials(await azureServiceTokenProvider.GetAccessTokenAsync("https://graph.microsoft.com/", tenantId).ConfigureAwait(false));
                AzureCredentials credentials = new AzureCredentials(serviceCreds, graphCreds, tenantId, AzureEnvironment.AzureGlobalCloud);


                //AzureCredentials credentials = SdkContext.AzureCredentialsFactory.FromSystemAssignedManagedServiceIdentity(
                // MSIResourceType.AppService,
                //AzureEnvironment.AzureGlobalCloud,
                // tenantId).WithDefaultSubscription(subscriptionId);

                //AzureCredentials credentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(
                //    clientId,
                //    clientSecret,
                //    tenantId,
                //    AzureEnvironment.AzureGlobalCloud).WithDefaultSubscription(subscriptionId);

    
                RestClient restClient = RestClient
                    .Configure()
                    .WithEnvironment(AzureEnvironment.AzureGlobalCloud)
                    .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                    .WithCredentials(credentials)
                    .Build();
                
                //var creds = new AzureCredentialsFactory().FromServicePrincipal(
                //    clientId,
                //    clientSecret,
                //    tenantId,
                //    AzureEnvironment.AzureGlobalCloud
                //    );


                //  var azure = Microsoft.Azure.Management.Fluent.Azure.Authenticate(creds);

                //Microsoft.Azure.Management.ResourceManager.Fluent.ResourceManagementClient resourceManagementClient = new Microsoft.Azure.Management.ResourceManager.Fluent.ResourceManagementClient(restClient);
                //resourceManagementClient.SubscriptionId = subscriptionId;

                //var rg = await resourceManagementClient.ResourceGroups.GetAsync("rgchell124");
                //// Get web app details
                WebSiteManagementClient webSiteManagementClient = new WebSiteManagementClient(restClient);
                webSiteManagementClient.SubscriptionId = subscriptionId;
          //      var settings = await azure.WithSubscription(subscriptionId).WebApps.Manager.Inner.WebApps.ListApplicationSettingsAsync("rgchell124", "func-connect133extractTransformsyh-dev1");
                var functionAppSettings =  await webSiteManagementClient.WebApps.ListApplicationSettingsAsync(resourceGroupName, functionName);
                
                var settingKeys = functionAppSettings.Properties.Keys.ToList();
                foreach (string appSetting in settingKeys)
                {
                    if (appSetting == "Topic")
                    {
                        functionAppSettings.Properties[appSetting] = "MyTopic4";
                        Console.WriteLine("Updated the topic");
                    }
                }
              
                // update settings
                await webSiteManagementClient.WebApps.UpdateApplicationSettingsAsync(resourceGroupName, functionName, functionAppSettings);

                //stop
                await webSiteManagementClient.WebApps.StopAsync(resourceGroupName, functionName);
                
                // start
                await webSiteManagementClient.WebApps.StartAsync(resourceGroupName, functionName);
               

                //var functionApp = await azure.WithSubscription(subscriptionId).WebApps.GetByResourceGroupAsync("rgchell124", "func-connect133extractTransformsyh-dev1");
                //var settings2 = functionApp.Inner.SiteConfig.AppSettings;
                //functionApp.Inner.SiteConfig.AppSettings = new List<NameValuePair> { new NameValuePair { Name = "Topic", Value = "Receive" } };
                //var appSettings = await functionApp.GetAppSettingsAsync();
                //functionApp.Stop();
                //foreach (IAppSetting appSetting in appSettings.Values)
                //{
                //    if (appSetting.Key == "Topic")
                //        Console.WriteLine(appSetting.Value);
                //}

                //SiteConfigResourceInner siteConfigResourceInner = await webSiteManagementClient.WebApps.GetConfigurationAsync("rgchell124", "func-connect133extractTransformsyh-dev1");


                //functionApp.Start();
            }
            catch (Exception ex)
            {

            }

        }

        private static async Task GetAppSettingsOfFunctionApp(AzureServiceTokenProvider azureServiceTokenProvider)
        {
            var subscriptionId = "56710d55-e631-4017-9ffc-380a1bfb05fe";
        //    var resourceGroupName = "rg-chella-dev";
        //    var providerNameSpace = "Microsoft.Web/sites";
        //    var resourceType = "config/appsettings";
        //    var resourceName = "func-connectextractTransformsyh-dev";
        //    var apiVersion = "2018-02-01";
        //    var resourceId = "/subscriptions/56710d55-e631-4017-9ffc-380a1bfb05fe/resourceGroups/rg-chella-dev/providers/Microsoft.Web/sites/func-connectextractTransformsyh-dev/config/appsettings";
        //    try
        //    {
               var serviceCreds = new TokenCredentials(await azureServiceTokenProvider.GetAccessTokenAsync("https://management.azure.com/", "4c91007c-ce23-4870-a0bc-3df6aff5f0da").ConfigureAwait(false));

            AzureCredentials credentials = new AzureCredentials(serviceCreds, serviceCreds, "", AzureEnvironment.AzureGlobalCloud);
        var resourceManagementClient =
            new Microsoft.Azure.Management.ResourceManager.ResourceManagementClient(serviceCreds) { SubscriptionId = subscriptionId };

         

            //        var result = await resourceManagementClient.Resources.GetByIdAsync(resourceId, apiVersion);


            //    }
            //    catch (Exception exp)
            //    {
            //        Console.WriteLine(exp);
            //    }

        }
    }
}
