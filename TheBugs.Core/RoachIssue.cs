using Octokit;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBugs
{
    public sealed class RoachIssue
    {
        public RoachIssueId Id { get; }
        public string Assignee { get; }
        public RoachMilestone Milestone { get; }
        public string Title { get; }
        public bool IsOpen { get; }
        public ImmutableArray<string> Labels;

        public int Number => Id.Number;
        public RoachRepoId RepoId => Id.RepoId;
        public string UriString => $"https://github.com/{RepoId.Owner}/{RepoId.Name}/issues/{Number}";
        public Uri Url => new Uri(UriString);

        public RoachIssue(RoachIssueId id, string assignee, RoachMilestone milestone, string title, bool isOpen, ImmutableArray<string> labels)
        {
            Id = id;
            Assignee = assignee;
            Milestone = milestone;
            Title = title;
            IsOpen = isOpen;
            Labels = labels;
        }

        public RoachIssue(Repository repo, Issue issue) : this(
            new RoachIssueId(repo, issue), 
            issue.Assignee?.Login ?? Constants.UnassignedName, 
            new RoachMilestone(issue.Milestone),
            issue.Title, 
            issue.State == ItemState.Open, 
            issue.Labels.Select(x => x.Name).ToImmutableArray())
        {

        }

        public override string ToString() => $"{Id.RepoId} {Id.Number} {Assignee}";
    }
}
