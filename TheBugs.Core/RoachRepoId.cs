using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBugs
{
    public struct RoachRepoId : IEquatable<RoachRepoId>
    {
        public string Owner { get; }
        public string Name { get; }

        public RoachRepoId(string owner, string name)
        {
            Owner = owner;
            Name = name;
        }

        public RoachRepoId(Repository repo) : this(repo.Owner.Login, repo.Name)
        {

        }

        public static bool operator==(RoachRepoId left, RoachRepoId right) => left.Owner == right.Owner && left.Name == right.Name;
        public static bool operator!=(RoachRepoId left, RoachRepoId right) => !(left == right);
        public bool Equals(RoachRepoId other) => this == other;
        public override bool Equals(object obj) => obj is RoachRepoId && Equals((RoachRepoId)obj);
        public override int GetHashCode() => Name?.GetHashCode() ?? 0;
        public override string ToString() => $"{Owner}/{Name}";
    }
}
