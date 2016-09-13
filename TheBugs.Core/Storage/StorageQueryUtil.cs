using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
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
    /// Responsible for running queries over our table storage.
    /// </summary>
    public sealed class StorageQueryUtil
    {
        private readonly CloudTable _issueTable;
        private readonly CloudTable _milestoneTable;

        public StorageQueryUtil(CloudTableClient tableClient) : this(
            tableClient.GetTableReference(TableNames.RoachIssueTable),
            tableClient.GetTableReference(TableNames.RoachMilestoneTable))
        {

        }

        public StorageQueryUtil(CloudTable issueTable, CloudTable milestoneTable)
        {
            Debug.Assert(issueTable.Name == TableNames.RoachIssueTable);
            Debug.Assert(milestoneTable.Name == TableNames.RoachMilestoneTable);

            _issueTable = issueTable;
            _milestoneTable = milestoneTable;
        }

        /// <summary>
        /// Get all issues in the specified label which don't have an assignee and / or a milestone.
        /// </summary>
        public async Task<List<RoachIssue>> GetTriageIssues(RoachRepoId repoId, string label, CancellationToken cancellationToken)
        {
            var unassignedFilter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition(nameof(TableEntity.PartitionKey), QueryComparisons.Equal, EntityKeyUtil.ToKey(repoId)),
                TableOperators.And,
                TableQuery.GenerateFilterCondition(nameof(RoachIssueEntity.Assignee), QueryComparisons.Equal, TheBugsConstants.UnassignedName));
            var unassignedList = await AzureUtil.QueryAsync(_issueTable, new TableQuery<RoachIssueEntity>().Where(unassignedFilter), cancellationToken);

            var unknownMilestoneFilter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition(nameof(TableEntity.PartitionKey), QueryComparisons.Equal, EntityKeyUtil.ToKey(repoId)),
                TableOperators.And,
                TableQuery.GenerateFilterConditionForInt(nameof(RoachIssueEntity.MilestoneNumber), QueryComparisons.Equal, RoachMilestoneId.NoneNumber));
            var unknownMilestoneList = await AzureUtil.QueryAsync(_issueTable, new TableQuery<RoachIssueEntity>().Where(unknownMilestoneFilter), cancellationToken);

            var list = new List<RoachIssue>();
            var hashSet = new HashSet<int>();
            foreach (var entity in unassignedList.Concat(unknownMilestoneList))
            {
                if (hashSet.Add(entity.Number) && entity.Labels.Contains(label))
                {
                    list.Add(entity.Issue);
                }
            }

            return list;
        }

        public async Task<List<RoachMilestone>> GetMilestones(RoachRepoId repoId, RoachItemFilter filter = RoachItemFilter.All, CancellationToken cancellationToken = default(CancellationToken))
        {
            var util = FilterUtil.PartitionKey(RoachMilestoneEntity.GetPartitionKey(repoId));
            switch (filter)
            {
                case RoachItemFilter.All:
                    // Nothing to do:
                    break;
                case RoachItemFilter.Closed:
                    util = util.And(FilterUtil.Column(nameof(RoachMilestoneEntity.IsOpen), false));
                    break;
                case RoachItemFilter.Open:
                    util = util.And(FilterUtil.Column(nameof(RoachMilestoneEntity.IsOpen), true));
                    break;
                default:
                    throw new Exception($"Bad enum value {filter}");
            }


            var list = await AzureUtil.QueryAsync<RoachMilestoneEntity>(
                _milestoneTable,
                util,
                cancellationToken);
            return list.Select(x => x.Milestone).OrderBy(x => x.Title).ToList();
        }

        public async Task<List<RoachIssue>> GetIssues(RoachRepoId repoId, string assignee, List<int> milestones, CancellationToken cancellationToken)
        {
            IEnumerable<RoachIssueEntity> list = await GetIssuesInMilestones(repoId, milestones, cancellationToken);
            if (assignee != null)
            {
                list = list.Where(x => x.Assignee == assignee);
            }

            return list.Select(x => x.Issue).ToList();
        }

        private async Task<List<RoachIssueEntity>> GetIssuesInMilestones(RoachRepoId repoId, List<int> milestones, CancellationToken cancellationToken)
        {
            if (milestones == null || milestones.Count == 0)
            {
                // Return all of the issues
                return await AzureUtil.QueryAsync<RoachIssueEntity>(_issueTable, FilterUtil.PartitionKey(RoachIssueEntity.GetPartitionKey(repoId)), cancellationToken);
            }

            var list = new List<RoachIssueEntity>();
            foreach (var milestone in milestones)
            {
                list.AddRange(await GetIssuesInMilestone(repoId, milestone, cancellationToken));
            }

            return list;
        }

        private async Task<List<RoachIssueEntity>> GetIssuesInMilestone(RoachRepoId repoId, int milestone, CancellationToken cancellationToken)
        {
            var filter = FilterUtil
                .PartitionKey(RoachIssueEntity.GetPartitionKey(repoId))
                .And(FilterUtil.Column(nameof(RoachIssueEntity.MilestoneNumber), milestone));
            return await AzureUtil.QueryAsync<RoachIssueEntity>(_issueTable, filter, cancellationToken);
        }
    }
}
