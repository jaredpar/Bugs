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

namespace TheBugs.Jobs
{
    public class Functions
    {
        public static async Task GitHubIssueChanged(
            [QueueTrigger(AzureConstants.QueueNames.IssueChanged)] string rawMessage,
            [Table(AzureConstants.TableNames.RoachIssueTable)] CloudTable table,
            CancellationToken cancellationToken)
        {
            var issueMessage = JsonConvert.DeserializeObject<IssueMessage>(rawMessage);

            var repoId = RoachRepoId.ParseFullName(issueMessage.RepoFullName);
            var issueId = new RoachIssueId(repoId, issueMessage.Number);
            var milestone = issueMessage.MilestoneTitle != null
                ? new RoachMilestone(repoId, issueMessage.MilestoneTitle, issueMessage.MilestoneNumber)
                : RoachMilestone.CreateNone(repoId);
            var isOpen = issueMessage.State == "open";
            var roachIssue = new RoachIssue(issueId, issueMessage.Assignee, milestone, issueMessage.Title, isOpen, issueMessage.Labels);
            var issueEntity = new RoachIssueEntity(roachIssue);
            var operation = TableOperation.InsertOrReplace(issueEntity);
            await table.ExecuteAsync(TableOperation.InsertOrReplace(issueEntity), cancellationToken);

            var milestoneEntity = new RoachMilestoneEntity(milestone);
            await table.ExecuteAsync(TableOperation.InsertOrReplace(milestoneEntity), cancellationToken);
        }
    }
}
