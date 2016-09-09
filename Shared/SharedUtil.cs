
using Microsoft.WindowsAzure.Storage;
using Octokit;
using System.Configuration;

namespace TheBugs
{
    internal static class SharedUtil
    {
        /// <summary>
        /// Milestone titles that we currently care about for display.
        /// </summary>
        internal static string[] DisplayMilestoneTitles => new[]
            {
                "2.0 (Preview 5)",
                "2.0 (RC)",
                "2.0 (RTM)",
            };

        /// <summary>
        /// Milestone titels that we currently care about for populating
        /// </summary>
        internal static string[] PopulateMilestoneTitles => new[] 
            {
                "Unknown",
                "2.0 (Preview 5)",
                "2.0 (RC)",
                "2.0 (RTM)",
            };

        internal static RoachRepoId RepoId => new RoachRepoId("dotnet", "roslyn");

        internal static string Label = "Area-Compilers";

        internal static GitHubClient CreateGitHubClient()
        {
            var client = new GitHubClient(new ProductHeaderValue("JaredsAmazingGithubBugClient"));
            var setting = ConfigurationManager.AppSettings[TheBugsConstants.GithubConnectionStringName];
            if (setting != null)
            {
                var items = setting.Split(':');
                client.Credentials = new Credentials(items[0], items[1]);
            }

            return client;
        }


        internal static CloudStorageAccount CreateStorageAccount()
        {
            return CloudStorageAccount.Parse(ConfigurationManager.AppSettings[TheBugsConstants.StorageConnectionStringName]);
        }
    }
}
