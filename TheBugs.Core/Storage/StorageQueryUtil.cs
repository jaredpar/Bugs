using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TheBugs.Storage
{
    /// <summary>
    /// Responsible for running queries over our table storage.
    /// </summary>
    public sealed class StorageQueryUtil
    {
        private readonly CloudTable _issueTable;
        private readonly CloudTable _milestoneTable;

        public StorageQueryUtil(CloudTableClient tableClient)
        {
            _issueTable = tableClient.GetTableReference(AzureConstants.TableNames.RoachIssueTable);
            _milestoneTable = tableClient.GetTableReference(AzureConstants.TableNames.RoachMilestoneTable);
        }

        public async Task<List<RoachMilestone>> GetMilestones(RoachRepoId repoId, CancellationToken cancellationToken)
        {
            var list = await AzureUtil.QueryAsync<RoachMilestoneEntity>(
                _milestoneTable,
                FilterUtil.PartitionKey(RoachMilestoneEntity.GetPartitionKey(repoId)),
                cancellationToken);
            return list.Select(x => x.Milestone).ToList();
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
