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
            foreach (var milestone in milestones)
            {
                await PopulateCore(new RoachMilestone(repoId, milestone), cancellationToken);
            }
        }

        /// <summary>
        /// Ensure the milestone is populated and up to date. 
        /// </summary>
        /// <remarks>
        /// There is no guarantee the issue table is 100% to date after this query.  Data is being compared across
        /// different data sources and times.  Hence it's going to be an approximation of correct.  Eventually though
        /// the data will be correct as time goes forward. 
        /// </remarks>
        private async Task PopulateCore(RoachMilestone milestone, CancellationToken cancellationToken)
        {
            var issueStatusList = await GetCurrentIssueStatus(milestone, cancellationToken);
            var issueStatusMap = issueStatusList.ToDictionary(x => x.Number);
            var githubIssueList = await _githubQueryUtil.GetIssuesInMilestone(milestone);

            var updateList = new List<RoachIssueEntity>();

            foreach (var issue in githubIssueList)
            {
                IssueStatus status;
                if (!issueStatusMap.TryGetValue(issue.Number, out status))
                {
                    updateList.Add(new RoachIssueEntity(new RoachIssue(milestone.RepoId, issue)));
                    continue;
                }

                status.Seen = true;
                if (status.UpdatedAt == null || issue.UpdatedAt == null || issue.UpdatedAt.Value > status.UpdatedAt.Value)
                {
                    updateList.Add(new RoachIssueEntity(new RoachIssue(milestone.RepoId, issue)));
                }
            }

            // Any issue not seen in this milestone needs to be deleted.  If it got moved to a new milestone then it will
            // be added back when that milestone is processed.
            var deleteList = issueStatusMap.Values.Where(x => !x.Seen).Select(x => x.EntityKey).ToList();

            // If there are no existing issues then have to consider the milestone just isn't present in storage yet.
            if (issueStatusList.Count == 0)
            {
                await AzureUtil.InsertBatchUnordered(_milestoneTable, new[] { new RoachMilestoneEntity(milestone) });
            }

            if (updateList.Count > 0)
            {
                await AzureUtil.InsertBatchUnordered(_issueTable, updateList);
            }

            if (deleteList.Count > 0)
            {
                await AzureUtil.DeleteBatchUnordered(_issueTable, deleteList);
            }
        }

        /// <summary>
        /// Query the status of all of the issues in the milestone at this time. 
        /// </summary>
        private async Task<List<IssueStatus>> GetCurrentIssueStatus(RoachMilestone milestone, CancellationToken cancellationToken)
        {
            var numberName = nameof(RoachIssueEntity.Number);
            var updatedAtName = nameof(RoachIssueEntity.UpdatedAtDateTime);
            var filter = FilterUtil
                .PartitionKey(EntityKeyUtil.ToKey(milestone.RepoId))
                .And(FilterUtil.Column(nameof(RoachIssueEntity.MilestoneNumber), milestone.Number));
            var query = new TableQuery<DynamicTableEntity>()
                .Where(filter.Filter)
                .Select(new[] { numberName, updatedAtName });
            var results = await AzureUtil.QueryAsync<DynamicTableEntity>(_issueTable, query, cancellationToken);

            var list = new List<IssueStatus>();
            foreach (var entity in results)
            {
                var number = entity[numberName].Int32Value.Value;
                DateTimeOffset? updatedAt = null;
                if (entity[updatedAtName].DateTime.HasValue)
                {
                    updatedAt = RoachIssueEntity.GetUpdatedAt(entity[updatedAtName].DateTime.Value);
                }

                list.Add(new IssueStatus(milestone.RepoId, number, updatedAt));
            }
            return list;
        }
    }
}
