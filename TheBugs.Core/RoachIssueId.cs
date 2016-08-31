using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBugs
{
    public struct RoachIssueId : IEquatable<RoachIssueId>
    {
        public RoachRepoId RepoId { get; }
        public int Number { get; }

        public RoachIssueId(RoachRepoId repoId, int number)
        {
            RepoId = repoId;
            Number = number;
        }

        public RoachIssueId(Repository repo, Issue issue) : this(new RoachRepoId(repo), issue.Number)
        {

        }

        public static bool operator==(RoachIssueId left, RoachIssueId right) => left.RepoId == right.RepoId && left.Number == right.Number;
        public static bool operator!=(RoachIssueId left, RoachIssueId right) => !(left == right);
        public bool Equals(RoachIssueId other) => this == other;
        public override bool Equals(object obj) => obj is RoachIssueId && Equals((RoachIssueId)obj);
        public override int GetHashCode() => Number;
        public override string ToString() => $"{RepoId.Owner}/{RepoId.Name}/{Number}";
    }
}
