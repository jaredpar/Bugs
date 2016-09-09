using Microsoft.WindowsAzure.Storage.Table;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static TheBugs.Storage.AzureConstants;

namespace TheBugs.Storage
{
    /// <summary>
    /// Used to bulk populate the storage tables. 
    /// </summary>
    public sealed class StoragePopulator
    {
        private readonly CloudTable _issueTable;
        private readonly CloudTable _milestoneTable;
        private readonly GitHubClient _githubClient;
        private readonly GitHubQueryUtil _githubQueryUtil;
        private readonly StorageQueryUtil _storageQueryUtil;

        public StoragePopulator(GitHubClient githubClient, CloudTableClient tableClient) : this(
            githubClient,
            tableClient.GetTableReference(TableNames.RoachIssueTable),
            tableClient.GetTableReference(TableNames.RoachMilestoneTable))
        {
        }

        public StoragePopulator(GitHubClient githubClient, CloudTable issueTable, CloudTable milestoneTable)
        {
            Debug.Assert(issueTable.Name == TableNames.RoachIssueTable);
            Debug.Assert(milestoneTable.Name == TableNames.RoachMilestoneTable);
            _githubClient = githubClient;
            _issueTable = issueTable;
            _milestoneTable = milestoneTable;
            _githubQueryUtil = new GitHubQueryUtil(_githubClient);
            _storageQueryUtil = new StorageQueryUtil(issueTable, milestoneTable);
        }

        /// <summary>
        /// Ensure issues in the specified milestones are populated and up to date in the tables. 
        /// </summary>
        public async Task Populate(RoachRepoId repoId, IEnumerable<int> milestones, CancellationToken cancellationToken = default(CancellationToken))
        {
            var all = await _githubQueryUtil.GetMilestones(repoId, milestones);
            await PopulateCore(repoId, all, cancellationToken);
        }


        public async Task Populate(RoachRepoId repoId, IEnumerable<string> milestones, CancellationToken cancellationToken = default(CancellationToken))
        {
            var all = await _githubQueryUtil.GetMilestones(repoId, milestones);
            await PopulateCore(repoId, all, cancellationToken);
        }

        private async Task PopulateCore(RoachRepoId repoId, IEnumerable<Milestone> milestones, CancellationToken cancellationToken = default(CancellationToken))
        { 
            // TODO: do a diff here.
            var issueList = new List<RoachIssue>();
            var milestoneMap = new Dictionary<int, RoachMilestone>();
            var githubRepo = await _githubClient.Repository.Get(repoId.Owner, repoId.Name);

            foreach (var milestone in milestones)
            {
                var issues = await _githubQueryUtil.GetIssuesInMilestone(githubRepo, milestone);
                issueList.AddRange(issues.Select(x => new RoachIssue(githubRepo, x)));
                milestoneMap[milestone.Number] = new RoachMilestone(githubRepo, milestone);
            }

            await AzureUtil.InsertBatchUnordered(_issueTable, issueList.Select(x => new RoachIssueEntity(x)));
            await AzureUtil.InsertBatchUnordered(_milestoneTable, milestoneMap.Values.Select(x => new RoachMilestoneEntity(x)));
        }
    }
}
