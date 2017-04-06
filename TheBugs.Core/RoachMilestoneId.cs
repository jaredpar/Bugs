using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBugs
{
    public struct RoachMilestoneId : IEquatable<RoachMilestoneId>
    {
        public const int NoneNumber = 0;
        public const int UnknownNumber = 2;

        public RoachRepoId RepoId { get; }
        public int Number { get; }

        public bool IsNone => Number == NoneNumber;

        public RoachMilestoneId(RoachRepoId repoId, int number)
        {
            RepoId = repoId;
            Number = number;
        }

        public RoachMilestoneId(RoachRepoId repoId, Milestone milestone)
        {
            RepoId = repoId;
            Number = milestone?.Number ?? NoneNumber;
        }

        public static RoachMilestoneId CreateNone(RoachRepoId repoId) => new RoachMilestoneId(repoId, NoneNumber);
        public static bool operator ==(RoachMilestoneId left, RoachMilestoneId right) => left.RepoId == right.RepoId && left.Number == right.Number;
        public static bool operator !=(RoachMilestoneId left, RoachMilestoneId right) => !(left == right);
        public bool Equals(RoachMilestoneId other) => this == other;
        public override bool Equals(object obj) => obj is RoachMilestoneId && Equals((RoachMilestoneId)obj);
        public override int GetHashCode() => Number;
        public override string ToString() => $"{RepoId.Owner}/{RepoId.Name}/{Number}";
    }
}
