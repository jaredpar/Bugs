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
        public string Milestone { get; }
        public string Title { get; }
        public bool IsOpen { get; }
        public Uri Url { get; }
        public ImmutableArray<string> Labels;
        public int Number => Id.Number;
        public RoachRepoId RepoId => Id.RepoId;

        public RoachIssue(RoachIssueId id, string assignee, string milestone, string title, bool isOpen, ImmutableArray<string> labels, Uri url)
        {
            Id = id;
            Assignee = assignee;
            Milestone = milestone;
            Title = title;
            IsOpen = isOpen;
            Labels = labels;
            Url = url;
        }

        public RoachIssue(Repository repo, Issue issue) : this(
            new RoachIssueId(repo, issue), 
            issue.Assignee?.Login ?? Constants.UnassignedName, 
            issue.Milestone.Title, 
            issue.Title, 
            issue.State == ItemState.Open, 
            issue.Labels.Select(x => x.Name).ToImmutableArray(),
            issue.Url)
        {

        }

        public override string ToString() => $"{Id.RepoId} {Id.Number} {Assignee}";
    }
}
