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
        public ImmutableArray<int> Milestones { get; }
        public ImmutableArray<RoachMilestone> AllMilestones { get; }

        public QueryModel(
            string actionName,
            ImmutableArray<RoachMilestone> allMilestones,
            string view = null,
            string assignee = null,
            ImmutableArray<int> milestones = default(ImmutableArray<int>))
        {
            ActionName = actionName;
            Milestones = milestones.IsDefault ? ImmutableArray<int>.Empty : milestones;
            AllMilestones = allMilestones;
            View = view;
            Assignee = assignee;
        }

        public static QueryModel Create(
            DataStorage storage, 
            string actionName,
            string assignee = null, 
            List<int> milestones = null,
            string view = null)
        {
            return new QueryModel(
                actionName,
                storage.Milestones,
                assignee: assignee,
                view: view,
                milestones: milestones == null ? ImmutableArray<int>.Empty : milestones.ToImmutableArray());
        }
    }
}