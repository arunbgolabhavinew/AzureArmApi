

using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Rest;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AzureAppServiceUpdate
{
    public static class AppServiceUpdate
    {
        readonly static string clientId = "clientId";
        readonly static string clientSecret = "clientSecret";
        readonly static string tenantId = "tenantId";
        readonly static string subscriptionId = "subscriptionId";
        readonly static string functionName = "functionName";
        readonly static string appServiceName = "appServiceName";
        readonly static string resourceGroupName = "resourceGroupName";

        public static async Task UpdateFunctionAppSettings()
        {
            try
            {
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


                // Get web app details
                WebSiteManagementClient webSiteManagementClient = new WebSiteManagementClient(restClient);
                webSiteManagementClient.SubscriptionId = subscriptionId;

                var functionAppSettings = await webSiteManagementClient.WebApps.ListApplicationSettingsAsync(resourceGroupName, functionName);

                var settingKeys = functionAppSettings.Properties.Keys.ToList();
                foreach (string appSetting in settingKeys)
                {
                    if (appSetting == "RetryCount")
                    {
                        functionAppSettings.Properties[appSetting] = "1";
                        Console.WriteLine("Updated the topic");
                    }
                    if (appSetting == "MaxAtempt")
                    {
                        functionAppSettings.Properties[appSetting] = "2";
                        Console.WriteLine("Updated the topic");
                    }
                }

                // update settings
                await webSiteManagementClient.WebApps.UpdateApplicationSettingsAsync(resourceGroupName, functionName, functionAppSettings);


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }


        public static async Task StartAppService()
        {
            try
            {
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


                // Get web app details
                WebSiteManagementClient webSiteManagementClient = new WebSiteManagementClient(restClient);
                webSiteManagementClient.SubscriptionId = subscriptionId;

                await webSiteManagementClient.WebApps.StartAsync(resourceGroupName, appServiceName);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        public static async Task StopAppService()
        {
            try
            {
                // This is for generic 
                //AzureCredentials credentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(
                //    clientId,
                //    clientSecret,
                //    tenantId,
                //    AzureEnvironment.AzureGlobalCloud).WithDefaultSubscription(subscriptionId);

                // Local Devlop or hosted application
                AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                var serviceCreds = new TokenCredentials(await azureServiceTokenProvider.GetAccessTokenAsync("https://management.azure.com/", tenantId).ConfigureAwait(false));
                var graphCreds = new TokenCredentials(await azureServiceTokenProvider.GetAccessTokenAsync("https://graph.microsoft.com/", tenantId).ConfigureAwait(false));
                AzureCredentials credentials = new AzureCredentials(serviceCreds, graphCreds, tenantId, AzureEnvironment.AzureGlobalCloud);


                RestClient restClient = RestClient
                    .Configure()
                    .WithEnvironment(AzureEnvironment.AzureGlobalCloud)
                    .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                    .WithCredentials(credentials)
                    .Build();


                // Get web app details
                WebSiteManagementClient webSiteManagementClient = new WebSiteManagementClient(restClient);
                webSiteManagementClient.SubscriptionId = subscriptionId;

                await webSiteManagementClient.WebApps.StopAsync(resourceGroupName, appServiceName);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
    }
}
