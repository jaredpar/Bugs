using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using System.IO;
using System.Collections.Immutable;
using FileMode = System.IO.FileMode;

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
                var issueQuery = GetIssueQuery();
                var list = await queryUtil.GetIssues(repo, issueQuery);
                var filtered = list.Where(x => IsMatchingLabels(x.Labels));

                using (var stream = File.Open(@"c:\users\jaredpar\Documents\issues.csv", FileMode.Create))
                using (var textWriter = new StreamWriter(stream))
                {
                    textWriter.WriteLine("Milestone,Assignee,Count,Url");
                    foreach (var issue in filtered)
                    {
                        textWriter.WriteLine($"{issue.Milestone.Title},{issue.Assignee.Login},1,{issue.Url}");
                    }
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

        private static IssueQuery GetIssueQuery()
        {
            var team = new[]
            {
                "jaredpar",
                "gafter",
                "VSadov",
                "cston",
                "tyoverby",
                "jcouv",
                "AlekseyTs",
                "agocke",
            };

            var milestones = new[]
            {
                "2.0 (Preview 5)",
                "2.0 (RC)",
                "2.0 (RTM)",
            };

            return new IssueQuery(team, milestones);
        }
    }

    internal class IssueQuery
    {
        internal ImmutableArray<string> Team { get; }
        internal ImmutableArray<string> Milestones { get; }

        internal IssueQuery(IEnumerable<string> team, IEnumerable<string> milestones)
        {
            Team = team.ToImmutableArray();
            Milestones = milestones.ToImmutableArray();
        }
    }

}
