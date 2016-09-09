using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using System.IO;
using System.Collections.Immutable;
using FileMode = System.IO.FileMode;
using TheBugs;
using System.Diagnostics;
using System.Configuration;
using TheBugs.Storage;
using Microsoft.WindowsAzure.Storage;

namespace DumpBugs
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            return Go().Result;
        }

        private static async Task<int> Go()
        {
            try
            {
                var client = SharedUtil.CreateGitHubClient();
                var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings[TheBugsConstants.StorageConnectionStringName]);
                AzureUtil.EnsureAzureResources(storageAccount);
                var tableClient = storageAccount.CreateCloudTableClient();

                var repo = await client.Repository.Get("dotnet", "roslyn");
                var storagePopulator = new StoragePopulator(client, tableClient);
                await storagePopulator.Populate(new RoachRepoId("dotnet", "roslyn"), SharedUtil.MilestoneTitles);

                return 0;
            }
            catch (Exception ex)
            {
                Console.Write($"{ex.Message}");
                return 1;
            }
        }

        private static bool IsMatchingLabels(IEnumerable<Label> labels)
        {
            foreach (var label in labels)
            {
                if (label.Name == "Documentation" || label.Name == "Test")
                {
                    return false;
                }
            }

            return true;
        }

        private static Credentials ReadCredentials()
        {
            var setting = ConfigurationManager.AppSettings[TheBugsConstants.GithubConnectionStringName];
            if (setting == null)
            {
                return null;
            }

            var items = setting.Split(':');
            return new Credentials(items[0], items[1]);
        }

        private static IEnumerable<string> GetMileStones()
        {
            var milestones = new[]
            {
                "2.0 (Preview 5)",
                "2.0 (RC)",
                "2.0 (RTM)",
            };
            return milestones;
        }
    }
}
