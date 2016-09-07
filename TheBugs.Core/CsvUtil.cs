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
                    textWriter.Write($"{data.RepoId.Owner},{data.RepoId.Name},");
                    textWriter.Write($"{data.Number},");
                    textWriter.Write($"{data.Assignee},");
                    textWriter.Write($"{Escape(data.Milestone.Title)},{data.Milestone.Number},");
                    textWriter.Write($"{Escape(data.Title)},");
                    textWriter.Write($"{data.IsOpen},");
                    textWriter.Write($"{EncodeList(data.Labels)}");
                    textWriter.WriteLine();
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
                    var index = 0;
                    Func<string> next = () => parts[index++];

                    var repoId = new RoachRepoId(next(), next());
                    var issueId = new RoachIssueId(repoId, int.Parse(next()));
                    var issue = new RoachIssue(
                        id: issueId,
                        assignee: next(),
                        milestone: new RoachMilestone(next(), int.Parse(next())),
                        title: next(),
                        isOpen: bool.Parse(next()),
                        labels: DecodeList(next()).ToImmutableArray());
                    list.Add(issue);
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
