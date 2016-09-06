using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Web;

namespace TheBugs.Utils
{
    internal sealed class Storage
    {
        // TODO: move to better storage.
        private static Storage s_instance;

        internal ImmutableArray<RoachIssue> Issues { get; }

        internal Storage(ImmutableArray<RoachIssue> issues)
        {
            Issues = issues;
        }

        internal static Storage GetOrCreate(HttpServerUtilityBase server)
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

        internal static IEnumerable<RoachIssue> Filter(IEnumerable<RoachIssue> issues, string filterName)
        {
            if (filterName != "jaredpar")
            {
                return issues;
            }

            return issues.Where(x => FilterUtil.CompilerTeam.IsIssue(x));
        }
   }
}