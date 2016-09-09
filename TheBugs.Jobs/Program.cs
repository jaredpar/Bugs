using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using TheBugs.Storage;

namespace TheBugs.Jobs
{
    internal static class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        internal static void Main()
        {
            var connectionString = CloudConfigurationManager.GetSetting(TheBugsConstants.StorageConnectionStringName);
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            AzureUtil.EnsureAzureResources(storageAccount);

            // Manually set the values vs. reading from connectionStrings.  Developing with connectionString
            // values is dangerous because you have to keep the password in the developer directory.  Can't use
            // relative source paths to find it above it.  So keep using appSettings here and just copy the 
            // values over.
            var config = new JobHostConfiguration();
            config.DashboardConnectionString = connectionString;
            config.StorageConnectionString = connectionString;
            config.UseTimers();

            var host = new JobHost(config);
            host.RunAndBlock();

        }
    }
}
