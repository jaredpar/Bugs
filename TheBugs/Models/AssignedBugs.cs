using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheBugs.Models
{
    public sealed class AssignedBugs
    {
        public string Assignee { get; }
        public string Filter { get; }
        public List<RoachIssue> Issues { get; }

        public AssignedBugs(string assignee, string filter, List<RoachIssue> issues)
        {
            Assignee = assignee;
            Filter = filter;
            Issues = issues;
        }
    }
}