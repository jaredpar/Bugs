using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBugs.Storage
{
    public sealed class RoachMilestoneEntity : TableEntity
    {
        public int Number { get; set; }
        public string Title { get; set; }
        public bool IsOpen { get; set; }

        public RoachRepoId RepoId => EntityKeyUtil.ParseRoachRepoIdKey(PartitionKey);
        public RoachMilestoneId Id => new RoachMilestoneId(RepoId, Number);
        public RoachMilestone Milestone => new RoachMilestone(Id, Title, IsOpen);

        public RoachMilestoneEntity()
        {

        }

        public RoachMilestoneEntity(RoachMilestone milestone)
        {
            PartitionKey = GetPartitionKey(milestone.RepoId);
            RowKey = GetRowKey(milestone);
            Number = milestone.Number;
            Title = milestone.Title;
            IsOpen = milestone.IsOpen;
        }

        public static string GetPartitionKey(RoachRepoId repoId) => EntityKeyUtil.ToKey(repoId);
        public static string GetRowKey(RoachMilestone milestone) => milestone.Number.ToString();

    }
}
