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

            public static IEnumerable<string> All()
            {
                yield return RoachIssueTable;
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
            public static IEnumerable<string> All()
            {
                yield break;
            }
        }
    }
}
