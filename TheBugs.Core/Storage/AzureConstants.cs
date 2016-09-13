using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBugs.Storage
{
    public static class AzureConstants
    {
        public static class TableNames
        {
            public const string RoachIssueTable = "RoachIssues";
            public const string RoachMilestoneTable = "RoachMilestones";
            public const string RoachStatusTable = "RoachStatusTable";

            public static IEnumerable<string> All()
            {
                yield return RoachIssueTable;
                yield return RoachMilestoneTable;
                yield return RoachStatusTable;
            }
        }

        public static class ContainerNames
        {
            public static IEnumerable<string> All()
            {
                yield break;
            }
        }

        public static class QueueNames
        {
            public const string IssueChanged = "issuechanged";

            public static IEnumerable<string> All()
            {
                yield return IssueChanged;
            }
        }
    }
}
