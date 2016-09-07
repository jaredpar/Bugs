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
        public string Title { get; }
        public int Number { get; }

        public RoachMilestone(string title, int number)
        {
            Title = title;
            Number = number;
        }

        public RoachMilestone(Milestone milestone)
        {
            Title = milestone.Title;
            Number = milestone.Number;
        }
    }
}
