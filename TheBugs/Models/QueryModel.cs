using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;
using TheBugs.Utils;

namespace TheBugs.Models
{
    public sealed class QueryModel
    {
        public string ActionName { get; }
        public string View { get; }
        public string Assignee { get; }
        public List<int> Milestones { get; }
        public List<RoachMilestone> AllMilestones { get; }

        public QueryModel(
            string actionName,
            List<RoachMilestone> allMilestones,
            string view = null,
            string assignee = null,
            List<int> milestones = null)
        {
            ActionName = actionName;
            Milestones = milestones ?? new List<int>();
            AllMilestones = allMilestones;
            View = view;
            Assignee = assignee;
        }
    }
}