using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheBugs.Utils
{
    public static class SpecificFilterUtil
    {
        public static class CompilerTeam
        {
            public static bool AssignedToTeam(RoachIssue issue)
            {
                switch (issue.Assignee.ToLower())
                {
                    case "jaredpar":
                    case "jcouv":
                    case "agocke":
                    case "alekseyts":
                    case "cston":
                    case "gafter":
                    case "tyoverby":
                    case "vsadov":
                        return true;
                    default:
                        return false;
                }
            }

            public static bool AssignedToArea(RoachIssue issue)
            {
                return issue.Labels.Contains("Area-Compilers");
            }

            public static bool IsIssue(RoachIssue issue)
            {
                if (IsNotBug(issue))
                {
                    return false;
                }

                if (AssignedToTeam(issue))
                {
                    return true;
                }

                if (AssignedToArea(issue) && issue.Assignee == TheBugsConstants.UnassignedName)
                {
                    return true;
                }

                return false;
            }

        }

        public static class IdeTeam
        {
            public static bool AssignedToTeam(RoachIssue issue)
            {
                switch (issue.Assignee.ToLower())
                {
                    case "pilchie":
                    case "brettfo":
                    case "cyrusnajmabadi":
                    case "dpoeschl":
                    case "jasonmalinowski":
                    case "kevinransom":
                    case "rchande":
                    case "sharwell":
                    case "dustincampbell":
                    case "kuhlenh":
                        return true;
                    default:
                        return false;
                }
            }

            public static bool AssignedToArea(RoachIssue issue)
            {
                return issue.Labels.Contains("Area-Ide");
            }

            public static bool IsIssue(RoachIssue issue)
            {
                if (IsNotBug(issue))
                {
                    return false;
                }

                if (AssignedToTeam(issue))
                {
                    return true;
                }

                if (AssignedToArea(issue) && issue.Assignee == TheBugsConstants.UnassignedName)
                {
                    return true;
                }

                return false;
            }

        }

        public static bool IsNotBug(RoachIssue issue)
        {
            if (!issue.IsOpen)
            {
                return true;
            }

            return
                issue.Labels.Contains("Documentation") ||
                issue.Labels.Contains("Question") ||
                issue.Labels.Contains("Area-Infrastructure");
        }

        public static IEnumerable<RoachIssue> Filter(
            IEnumerable<RoachIssue> issues,
            string view)
        {
            if (view != null)
            {
                switch (view)
                {
                    case "jaredpar":
                        issues = issues.Where(x => SpecificFilterUtil.CompilerTeam.IsIssue(x));
                        break;
                    case "me@pilchie.com":
                        issues = issues.Where(x => SpecificFilterUtil.IdeTeam.IsIssue(x));
                        break;
                }
            }

            return issues;
        }
    }
}