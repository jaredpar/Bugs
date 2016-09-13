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
            var milestoneId = new RoachMilestoneId(repoId, issueMessage.MilestoneNumber);
            var isOpen = issueMessage.State == "open";
            var updatedAt = issueMessage.UpdatedAt != null
                ? DateTimeOffset.Parse(issueMessage.UpdatedAt)
                : (DateTimeOffset?)null;
            var roachIssue = new RoachIssue(issueId, issueMessage.Assignee ?? TheBugsConstants.UnassignedName, milestoneId, issueMessage.Title, isOpen, issueMessage.Labels, updatedAt);
            var issueEntity = new RoachIssueEntity(roachIssue);
            var operation = TableOperation.InsertOrReplace(issueEntity);
            await table.ExecuteAsync(TableOperation.InsertOrReplace(issueEntity), cancellationToken);
        }

        /// <summary>
        /// Milestone information still does not come down as a part of events.  This function catches these types of 
        /// update by doing a 'since' query on GitHub and bulk updating all of the changed values.  
        /// </summary>
        public static async Task GithubPopulateIssuesSince(
            [TimerTrigger("0 0/1 * * * *")] TimerInfo timerInfo,
            [Table(TableNames.RoachIssueTable)] CloudTable issueTable,
            [Table(TableNames.RoachMilestoneTable)] CloudTable milestoneTable,
            [Table(TableNames.RoachStatusTable)] CloudTable statusTable,
            TextWriter logger,
            CancellationToken cancellationToken)
        {
            var client = SharedUtil.CreateGitHubClient();
            var storagePopulator = new StoragePopulator(client, issueTable, milestoneTable);

            // TODO: Need to make this adaptable to all repos, not just dotnet/roslyn
            var allRepos = new[] { SharedUtil.RepoId };
            foreach (var repo in allRepos)
            {
                var statusEntity = await AzureUtil.QueryAsync<RoachStatusEntity>(statusTable, RoachStatusEntity.GetEntityKey(repo), cancellationToken);
                if (statusEntity == null || statusEntity.LastBulkUpdate.Value == null)
                {
                    logger.WriteLine($"Repo {repo.Owner}/{repo.Name} does not have a status entry.  Cannot do a since update.");
                    return;
                }

                var before = DateTimeOffset.UtcNow;
                await storagePopulator.PopulateIssuesSince(repo, statusEntity.LastBulkUpdate.Value, cancellationToken);

                // Given there are no events for milestones need to do a bulk update here. 
                await storagePopulator.PopulateMilestones(repo, cancellationToken);

                statusEntity.SetLastBulkUpdate(before);
                await statusTable.ExecuteAsync(TableOperation.Replace(statusEntity), cancellationToken);
            }
        }
    }
}
