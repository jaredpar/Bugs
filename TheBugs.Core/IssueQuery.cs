using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBugs
{
    public sealed class IssueQuery
    {
        internal ImmutableArray<string> Team { get; }
        internal ImmutableArray<string> Milestones { get; }

        public IssueQuery(IEnumerable<string> team, IEnumerable<string> milestones)
        {
            Team = team.ToImmutableArray();
            Milestones = milestones.ToImmutableArray();
        }
    }
}
