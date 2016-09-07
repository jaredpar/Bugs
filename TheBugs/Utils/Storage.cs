using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Web;

namespace TheBugs.Utils
{
    public sealed class Storage
    {
        // TODO: move to better storage.
        private static Storage s_instance;

        public ImmutableArray<RoachIssue> Issues { get; }
        public ImmutableArray<RoachMilestone> Milestones { get; }
        public ImmutableDictionary<int, RoachMilestone> MilestoneMap { get; }

        public Storage(ImmutableArray<RoachIssue> issues)
        {
            Issues = issues;
            MilestoneMap = issues
                .Select(x => x.Milestone)
                .ToImmutableDictionary(x => x.Number);
            Milestones = MilestoneMap.Values.ToImmutableArray();
        }

        public IEnumerable<RoachIssue> Filter(
            string assignee,
            string view,
            IList<int> milestones)
        {
            var issues = view == "jaredpar"
                ? Issues.Where(x => FilterUtil.CompilerTeam.IsIssue(x))
                : Issues;

            if (!string.IsNullOrEmpty(assignee))
            {
                issues = issues.Where(x => x.Assignee == assignee);
            }

            if (milestones.Count > 0)
            {
                issues = issues.Where(x => milestones.Contains(x.Milestone.Number));
            }

            return issues;
        }

        public static Storage GetOrCreate(HttpServerUtilityBase server)
        {
            if (s_instance != null)
            {
                return s_instance;
            }

            s_instance = Create(server);
            return s_instance;
        }

        private static Storage Create(HttpServerUtilityBase server)
        {
            var path = server.MapPath("~/App_Data/issues.csv");
            using (var stream = File.Open(path, FileMode.Open))
            {
                var issues = CsvUtil.Import(stream);
                return new Storage(issues.ToImmutableArray());
            }
        }
   }
}