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
                var client = new GitHubClient(new ProductHeaderValue("JaredsAmazingGithubBugClient"));
                client.Credentials = ReadCredentials();

                var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings[Constants.StorageConnectionStringName]);
                AzureUtil.EnsureAzureResources(storageAccount);
                var table = storageAccount.CreateCloudTableClient().GetTableReference(AzureConstants.TableNames.RoachIssueTable);

                var queryUtil = new QueryUtil(client);
                var repo = await client.Repository.Get("dotnet", "roslyn");
                var list = new List<RoachIssue>();

                foreach (var title in GetMileStones())
                {
                    var milestone = await queryUtil.GetMilestone(repo, title);
                    var request = new RepositoryIssueRequest()
                    {
                        Milestone = $"{milestone.Number}",
                        Filter = IssueFilter.All,
                    };

                    var issues = await queryUtil.GetIssuesInMilestone(repo, milestone);
                    foreach (var issue in issues)
                    {
                        list.Add(new RoachIssue(repo, issue));
                    }
                }

                Console.WriteLine("Inserting into Azure tables");
                var entityList = list.Select(x => new RoachIssueEntity(x)).ToList();
                await AzureUtil.InsertBatchUnordered(table, entityList);

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
            var setting = ConfigurationManager.AppSettings[Constants.GithubConnectionStringName];
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
