using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
                    textWriter.WriteLine($"{data.RepoId.Owner},{data.RepoId.Name},{data.Number},{data.Assignee},{data.Milestone},{Escape(data.Title)},{data.IsOpen},{EncodeList(data.Labels)},{data.Url}");
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
                    var parts = line.Split(new[] { ',' }, count: 9);
                    var repoId = new RoachRepoId(parts[0], parts[1]);
                    var issueId = new RoachIssueId(repoId, int.Parse(parts[2]));
                    var data = new RoachIssue(issueId, parts[3], parts[4], parts[5], bool.Parse(parts[6]), DecodeList(parts[7]).ToImmutableArray(), new Uri(parts[8]));
                    list.Add(data);
                    line = textReader.ReadLine();
                }

                return list;
            }
        }

        // TODO: Need an unescape. 
        private static string Escape(string str)
        {
            if (!str.Contains(','))
            {
                return str;
            }

            return str.Replace(',', ' ');
        }

        // TODO: so hacky, doesn't account for # in string values.  Not therce for labels so skipping 
        // complexity for now
        private static string EncodeList(IList<string> all)
        {
            if (all.Count == 0)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            var first = true;
            foreach (var item in all)
            {
                if (!first)
                {
                    builder.Append('#');
                }
                builder.Append(Escape(item));
                first = false;
            }

            return builder.ToString();
        }

        private static IEnumerable<string> DecodeList(string all)
        {
            var parts = all.Split('#');
            return parts;
        }
    }
}
