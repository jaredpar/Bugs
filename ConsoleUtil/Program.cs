using Microsoft.WindowsAzure.Storage;
using Octokit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheBugs;
using TheBugs.Storage;

namespace ConsoleUtil
{
    internal static class Program
    {
        internal static int Main(string[] args)
        {
            return Go().Result;
        }

        internal static async Task<int> Go()
        {
            try
            {
                var client = SharedUtil.CreateGitHubClient();
                var storageAccount = SharedUtil.CreateStorageAccount();

                // await DumpHooks(client);
                // await DumpMilestones(client);
                // await PrintRateLimits(client);
                // await TestRateLimits(client, storageAccount);
                // await FixNulls(storageAccount);
                await DumpSince(client);
               
                return 0;
            }
            catch (Exception ex)
            {
                Console.Write($"{ex.Message}");
                return 1;
            }
        }

        private static async Task PrintRateLimits(GitHubClient client)
        {
            var limit = await client.Miscellaneous.GetRateLimits();
            PrintRateLimits("Core Rate", limit.Rate);
            PrintRateLimits("Search Rate", limit.Resources.Search);
        }

        private static void PrintRateLimits(string title, RateLimit rate)
        {
            Console.WriteLine(title);
            Console.WriteLine($"\tLimit: {rate.Limit}");
            Console.WriteLine($"\tRemaining: {rate.Remaining}");
        }

        private static async Task TestRateLimits(GitHubClient client, CloudStorageAccount storageAccount)
        {
            Console.WriteLine("Before");
            await PrintRateLimits(client);

            var populator = new StoragePopulator(client, storageAccount.CreateCloudTableClient());
            await populator.Populate(SharedUtil.RepoId, SharedUtil.PopulateMilestoneTitles);

            Console.WriteLine("After");
            await PrintRateLimits(client);
        }

        private static async Task FixNulls(CloudStorageAccount storageAccount)
        {
            var table = storageAccount.CreateCloudTableClient().GetTableReference(AzureConstants.TableNames.RoachIssueTable);
            var filter = FilterUtil.PartitionKey(EntityKeyUtil.ToKey(SharedUtil.RepoId));
            var list = await AzureUtil.QueryAsync<RoachIssueEntity>(table, filter);
            var bad = list.Where(x => x.Assignee == null).ToList();
            await AzureUtil.DeleteBatchUnordered(table, bad);
        }

        private static async Task DumpMilestones(GitHubClient client)
        {
            var milestones = await client.Issue.Milestone.GetAllForRepository("dotnet", "roslyn");
            foreach (var m in milestones)
            {
                Console.WriteLine($"{m.Number} - {m.Title}");
            }
        }

        private static async Task DumpHooks(GitHubClient client)
        {
            var hooks = await client.Repository.Hooks.GetAll("dotnet", "roslyn");
            foreach (var hook in hooks)
            {
                string url;
                if (!hook.Config.TryGetValue("url", out url))
                {
                    url = "<not present>";
                }

                Console.WriteLine($"{hook.Id} {hook.Name}");
                Console.WriteLine($"\tGitHub Url: {hook.Url}");
                Console.WriteLine($"\tReal Url: {url}");
                Console.WriteLine($"\tEvents:");
                foreach (var e in hook.Events)
                {
                    Console.WriteLine($"\t\t{e}");
                }
            }
        }

        private static async Task DumpSince(GitHubClient client)
        {
            var date = DateTimeOffset.UtcNow - TimeSpan.FromHours(2);
            var request = new RepositoryIssueRequest()
            {
                Since = date
            };
            var list = await client.Issue.GetAllForRepository("dotnet", "roslyn", request);
            foreach (var item in list)
            {
                Console.WriteLine($"{item.Number} - {item.Title}");
            }
       } 
    }
}
