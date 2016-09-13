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
        public const string NoneTitle = "<none>";

        public RoachMilestoneId Id { get; }
        public string Title { get; }
        public bool IsOpen { get; }

        public RoachRepoId RepoId => Id.RepoId;
        public int Number => Id.Number;

        public RoachMilestone(RoachMilestoneId id, string title, bool isOpen)
        {
            Id = id;
            Title = title;
            IsOpen = isOpen;
        }

        public RoachMilestone(RoachRepoId repoId, int number, string title, bool isOpen) : this(new RoachMilestoneId(repoId, number), title, isOpen)
        {

        }

        public RoachMilestone(RoachRepoId repoId, Milestone milestone)
        {
            if (milestone != null)
            {
                Id = new RoachMilestoneId(repoId, milestone.Number);
                Title = milestone.Title;
                IsOpen = milestone.State == ItemState.Open;
            }
            else
            {
                Id = RoachMilestoneId.CreateNone(repoId);
                Title = NoneTitle;
                IsOpen = true;
            }
        }

        public RoachMilestone(Repository repo, Milestone milestone) : this(new RoachRepoId(repo), milestone)
        {

        }

    }
}
