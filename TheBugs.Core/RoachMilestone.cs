using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBugs
{
    public struct RoachMilestone
    {
        public const int NoneNumber = 0;

        public RoachRepoId RepoId { get; }
        public string Title { get; }
        public int Number { get; }

        public RoachMilestone(RoachRepoId repoId, string title, int number)
        {
            RepoId = repoId;
            Title = title;
            Number = number;
        }

        public RoachMilestone(RoachRepoId repoId, Milestone milestone)
        {
            RepoId = repoId;
            Title = milestone.Title;
            Number = milestone.Number;
        }

        public RoachMilestone(Repository repo, Milestone milestone) : this(new RoachRepoId(repo), milestone)
        {

        }

        public static RoachMilestone CreateNone(RoachRepoId repoId)
        {
            return new RoachMilestone(repoId, "", NoneNumber);
        }
    }
}
