using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TheBugs.Storage.AzureConstants;

namespace TheBugs.Storage
{
    /// <summary>
    /// Class which helps initialize a repo on table storage.
    /// </summary>
    public sealed class RepoInitUtil
    {
        private readonly RoachRepoId _repoId;
        private readonly GitHubClient _client;
        private readonly CloudTable _issueTable;
        private readonly CloudTable _milestoneTable;
        private readonly CloudTable _statusTable;

        private RepoInitUtil(RoachRepoId id, GitHubClient githubClient, CloudTableClient client)
        {
            _repoId = id;
            _client = githubClient;
            _issueTable = client.GetTableReference(TableNames.RoachIssueTable);
            _milestoneTable = client.GetTableReference(TableNames.RoachMilestoneTable);
            _statusTable = client.GetTableReference(TableNames.RoachStatusTable);
        }

        public static async Task Initialize(RoachRepoId id, GitHubClient githubClient, CloudStorageAccount account, TextWriter logger)
        {
            var util = new RepoInitUtil(id, githubClient, account.CreateCloudTableClient());
            await util.Go(logger);
        }

        private async Task Go(TextWriter logger)
        {
            var before = DateTimeOffset.UtcNow;
            await PopulateIssues(logger);
            await PopulateMilestones(logger);

            logger.WriteLine("Updating the status table");
            var entity = new RoachStatusEntity(_repoId, before);
            await _statusTable.ExecuteAsync(TableOperation.InsertOrReplace(entity));
        }

        private async Task PopulateIssues(TextWriter logger)
        {
            var util = new GitHubQueryUtil(_client);
            var request = new RepositoryIssueRequest()
            {
                State = ItemStateFilter.All
            };

            logger.WriteLine($"Getting issues");
            var issues = await util.GetIssues(_repoId, request);
            var list = new List<RoachIssueEntity>();
            foreach (var issue in issues)
            {
                var entity = new RoachIssueEntity(new RoachIssue(_repoId, issue));
                list.Add(entity);
            }

            logger.WriteLine("Populating issues table storage");
            await AzureUtil.InsertBatchUnordered(_issueTable, list);
        }

        private async Task PopulateMilestones(TextWriter logger)
        {
            logger.WriteLine("Populating milestones table storage");
            var util = new StoragePopulator(_client, _issueTable, _milestoneTable);
            await util.PopulateMilestones(_repoId);
        }
    }
}
