using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBugs
{
    public static class CsvUtil
    {
        public static void Export(Stream stream, IEnumerable<RoachIssue> issues)
        {
            using (var textWriter = new StreamWriter(stream))
            {
                foreach (var data in issues)
                {
                    textWriter.WriteLine($"{data.RepoId.Owner},{data.RepoId.Name},{data.Number},{data.Assignee},{data.Milestone},{Escape(data.Title)},{data.IsOpen},{data.Url}");
                }
            }
        }

        public static List<RoachIssue> Import(Stream stream)
        {
            using (var textReader = new StreamReader(stream))
            {
                var list = new List<RoachIssue>();
                var line = textReader.ReadLine();
                while (line != null)
                {
                    var parts = line.Split(new[] { ',' }, count: 8);
                    var repoId = new RoachRepoId(parts[0], parts[1]);
                    var issueId = new RoachIssueId(repoId, int.Parse(parts[2]));
                    var data = new RoachIssue(issueId, parts[3], parts[4], parts[5], bool.Parse(parts[6]), new Uri(parts[7]));
                    list.Add(data);
                    line = textReader.ReadLine();
                }

                return list;
            }
        }

        private static string Escape(string str)
        {
            if (!str.Contains(','))
            {
                return str;
            }

            return str.Replace(',', ' ');
        }
    }
}
