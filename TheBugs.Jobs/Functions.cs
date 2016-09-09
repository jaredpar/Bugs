using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using TheBugs.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System.Threading;
using static TheBugs.Storage.AzureConstants;

namespace TheBugs.Jobs
{
    public class Functions
    {
        public static async Task GitHubIssueChanged(
            [QueueTrigger(QueueNames.IssueChanged)] string rawMessage,
            [Table(TableNames.RoachIssueTable)] CloudTable table,
            CancellationToken cancellationToken)
        {
            var issueMessage = JsonConvert.DeserializeObject<IssueMessage>(rawMessage);

            var repoId = RoachRepoId.ParseFullName(issueMessage.RepoFullName);
            var issueId = new RoachIssueId(repoId, issueMessage.Number);
            var milestone = issueMessage.MilestoneTitle != null
                ? new RoachMilestone(repoId, issueMessage.MilestoneTitle, issueMessage.MilestoneNumber)
                : RoachMilestone.CreateNone(repoId);
            var isOpen = issueMessage.State == "open";
            var updatedAt = issueMessage.UpdatedAt != null
                ? DateTimeOffset.Parse(issueMessage.UpdatedAt)
                : (DateTimeOffset?)null;
            var roachIssue = new RoachIssue(issueId, issueMessage.Assignee, milestone, issueMessage.Title, isOpen, issueMessage.Labels, updatedAt);
            var issueEntity = new RoachIssueEntity(roachIssue);
            var operation = TableOperation.InsertOrReplace(issueEntity);
            await table.ExecuteAsync(TableOperation.InsertOrReplace(issueEntity), cancellationToken);

            var milestoneEntity = new RoachMilestoneEntity(milestone);
            await table.ExecuteAsync(TableOperation.InsertOrReplace(milestoneEntity), cancellationToken);
        }

        public static async Task GithubPopulate(
            [TimerTrigger("0 0/5 * * * *")] TimerInfo timerInfo,
            [Table(TableNames.RoachIssueTable)] CloudTable issueTable,
            [Table(TableNames.RoachMilestoneTable)]CloudTable milestoneTable,
            CancellationToken cancellationToken)
        {
            var client = SharedUtil.CreateGitHubClient();
            var storagePopulator = new StoragePopulator(client, issueTable, milestoneTable);
            await storagePopulator.Populate(SharedUtil.RepoId, SharedUtil.MilestoneTitles, cancellationToken);
        }
    }
}
