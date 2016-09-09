using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBugs.Jobs
{
    public sealed class IssueMessage
    {
        public int Number { get; set; }
        public string Title { get; set; }
        public string MilestoneTitle { get; set; }
        public int MilestoneNumber { get; set; }
        public string Assignee { get; set; }
        public string UpdatedAt { get; set; }

        /// <summary>
        /// This will have the name in the form dotnet/roslyn
        /// </summary>
        public string RepoFullName { get; set; }

        public string State { get; set; }

        public string[] Labels { get; set; }

    }
}
