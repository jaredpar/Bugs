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

                var documentPath = @"..\..\..\TheBugs\App_Data\issues.csv";
                using (var stream = File.Open(documentPath, FileMode.Create))
                {
                    CsvUtil.Export(stream, list);
                }

                // Sanity check
                using (var stream = File.Open(documentPath, FileMode.Open))
                {
                    var all = CsvUtil.Import(stream);
                    Debug.Assert(all.Count == list.Count);
                }

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
            var text = File.ReadAllText(@"c:\users\jaredpar\github.txt").Trim();
            var items = text.Split(':');
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
