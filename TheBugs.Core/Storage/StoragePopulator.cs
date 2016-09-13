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
        #region IssueStatus

        private sealed class IssueStatus
        {
            internal RoachRepoId RepoId { get; }
            internal int Number { get; }
            internal DateTimeOffset? UpdatedAt { get; }
            internal bool Seen { get; set; }
            internal EntityKey EntityKey => RoachIssueEntity.GetEntityKey(new RoachIssueId(RepoId, Number));

            internal IssueStatus(RoachRepoId repoId, int number, DateTimeOffset? updatedAt)
            {
                RepoId = repoId;
                Number = number;
                UpdatedAt = updatedAt;
            }
        }

        #endregion

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
        /// Populates the issues tables to reflect changes that occured since the see cref="since"/> parameter.
        /// </summary>
        public async Task PopulateIssuesSince(RoachRepoId repoId, DateTimeOffset since, CancellationToken cancellationtoken = default(CancellationToken))
        {
            var changedIssues = await _githubQueryUtil.GetIssuesChangedSince(repoId, since);
            var issueList = new List<RoachIssueEntity>(capacity: changedIssues.Count);

            foreach (var changedIssue in changedIssues)
            {
                var issue = new RoachIssue(repoId, changedIssue);
                issueList.Add(new RoachIssueEntity(issue));
            }

            // TODO: Need to thread through the CancellationToken here.
            await AzureUtil.InsertBatchUnordered(_issueTable, issueList);
        }

        /// <summary>
        /// Populates the milestone tableo to be equivalent to the current set of milestones on GitHub.
        /// </summary>
        public async Task PopulateMilestones(RoachRepoId repoId, CancellationToken cancellationtoken = default(CancellationToken))
        {
            var milestones = await _storageQueryUtil.GetMilestones(repoId, RoachItemFilter.All, cancellationtoken);
            var map = milestones.ToDictionary(x => x.Number);

            var list = new List<RoachMilestoneEntity>();
            foreach (var milestone in await _githubQueryUtil.GetMilestones(repoId))
            {
                var isOpen = milestone.State == ItemState.Open;

                RoachMilestone r;
                if (!map.TryGetValue(milestone.Number, out r) || 
                    r.Title != milestone.Title ||
                    r.IsOpen != isOpen)
                {
                    r = new RoachMilestone(repoId, milestone.Number, milestone.Title, isOpen);
                    list.Add(new RoachMilestoneEntity(r));
                }
            }

            if (list.Count > 0)
            {
                // TODO: Need to thread through the CancellationToken here.
                await AzureUtil.InsertBatchUnordered(_milestoneTable, list);
            }
        }
    }
}
