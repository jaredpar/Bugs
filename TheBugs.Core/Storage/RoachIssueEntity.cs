﻿using Microsoft.WindowsAzure.Storage.Table;
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
        public string MilestoneTitle { get; set; }
        public int MilestoneNumber { get; set; }
        public string Title { get; set; }
        public bool IsOpen { get; set; }
        public string LabelsRaw { get; set; }

        public RoachRepoId RepoId => EntityKeyUtil.ParseRoachRepoIdKey(PartitionKey);
        public RoachMilestone Milestone => new RoachMilestone(RepoId, MilestoneTitle, MilestoneNumber);
        public IEnumerable<string> Labels => LabelsRaw.Split('#');
        public RoachIssueId IssueId => new RoachIssueId(RepoId, Number);
        public RoachIssue Issue => new RoachIssue(IssueId, Assignee, Milestone, Title, IsOpen, Labels);

        public RoachIssueEntity()
        {

        }

        public RoachIssueEntity(RoachIssue issue)
        {
            PartitionKey = GetPartitionKey(issue.RepoId);
            RowKey = GetRowKey(issue.Id);

            Number = issue.Number;
            Assignee = issue.Assignee;
            MilestoneTitle = issue.Milestone.Title;
            MilestoneNumber = issue.Milestone.Number;
            Title = issue.Title;
            IsOpen = issue.IsOpen;
            LabelsRaw = string.Join("#", issue.Labels);
        }

        public static string GetPartitionKey(RoachRepoId repoId) => EntityKeyUtil.ToKey(repoId);
        public static string GetRowKey(int number) => number.ToString();
        public static string GetRowKey(RoachIssueId id) => GetRowKey(id.Number).ToString();
    }
}
