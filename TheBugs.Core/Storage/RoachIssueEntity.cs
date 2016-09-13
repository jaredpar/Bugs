using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBugs.Storage
{
    public sealed class RoachIssueEntity : TableEntity
    {
        public int Number { get; set; }
        public string Assignee { get; set; }
        public int MilestoneNumber { get; set; }
        public string Title { get; set; }
        public bool IsOpen { get; set; }
        public string LabelsRaw { get; set; }
        public DateTime UpdatedAtDateTime { get; set; }

        public RoachRepoId RepoId => EntityKeyUtil.ParseRoachRepoIdKey(PartitionKey);
        public RoachMilestoneId MilestoneId => new RoachMilestoneId(RepoId, MilestoneNumber);
        public IEnumerable<string> Labels => LabelsRaw != null ? LabelsRaw.Split('#') : new string[] { };
        public RoachIssueId IssueId => new RoachIssueId(RepoId, Number);
        public DateTimeOffset? UpdatedAt => UpdatedAtDateTime == default(DateTime) ? (DateTimeOffset?)null : UpdatedAtDateTime;
        public RoachIssue Issue => new RoachIssue(IssueId, Assignee ?? TheBugsConstants.UnassignedName, MilestoneId, Title, IsOpen, Labels, UpdatedAt);

        public RoachIssueEntity()
        {

        }

        public RoachIssueEntity(RoachIssue issue)
        {
            PartitionKey = GetPartitionKey(issue.RepoId);
            RowKey = GetRowKey(issue.Id);

            Number = issue.Number;
            Assignee = issue.Assignee;
            MilestoneNumber = issue.MilestoneId.Number;
            Title = issue.Title;
            IsOpen = issue.IsOpen;
            LabelsRaw = string.Join("#", issue.Labels);
            UpdatedAtDateTime = (issue.UpdatedAt ?? default(DateTimeOffset)).UtcDateTime;
        }

        public static string GetPartitionKey(RoachRepoId repoId) => EntityKeyUtil.ToKey(repoId);
        public static string GetRowKey(int number) => number.ToString();
        public static string GetRowKey(RoachIssueId id) => GetRowKey(id.Number).ToString();
        public static EntityKey GetEntityKey(RoachIssueId id) => new EntityKey(GetPartitionKey(id.RepoId), GetRowKey(id.Number));
        public static DateTimeOffset? GetUpdatedAt(DateTime updatedAtDateTime) => updatedAtDateTime == default(DateTime) ? (DateTimeOffset?)null : updatedAtDateTime;
    }
}
