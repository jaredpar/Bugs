using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TheBugs
{
    public sealed class GitHubQueryUtil
    {
        internal const int DefaultPageSize = 100;

        private readonly GitHubClient _client;

        public object ConfigurationManager { get; private set; }

        public GitHubQueryUtil(GitHubClient client)
        {
            _client = client;
        }

        public async Task<List<Issue>> GetIssues(Repository repo, IssueQuery query)
        {
            // TODO: need to handle unassigned bugs too. 

            var list = new List<Issue>();
            foreach (var member in query.Team)
            {
                foreach (var title in query.Milestones)
                {
                    var milestone = await GetMilestone(repo, title);
                    var request = new RepositoryIssueRequest();
                    request.Milestone = $"{milestone.Number}";
                    request.Assignee = member;
                    request.State = ItemStateFilter.Open;
                    list.AddRange(await GetIssues(repo, request));
                }
            }

            return list;
        }

        public async Task<IReadOnlyList<Milestone>> GetMilestones(RoachRepoId repoId)
        {
            return await _client.Issue.Milestone.GetAllForRepository(repoId.Owner, repoId.Name);
        }

        public async Task<IEnumerable<Milestone>> GetMilestones(RoachRepoId repoId, IEnumerable<int> milestoneNumbers)
        {
            var all = await GetMilestones(repoId);
            return all.Where(x => milestoneNumbers.Contains(x.Number));
        }

        public async Task<IEnumerable<Milestone>> GetMilestones(RoachRepoId repoId, IEnumerable<string> milestoneTitles)
        {
            var all = await GetMilestones(repoId);
            return all.Where(x => milestoneTitles.Contains(x.Title));
        }

        // TODO: delete rest or make them in terms of our API primitivess
        public async Task<List<Issue>> GetIssuesInMilestone(Repository repo, string milestoneTitle)
        {
            var milestone = await GetMilestone(repo, milestoneTitle);
            return await GetIssuesInMilestone(repo, milestone);
        }

        public async Task<List<Issue>> GetIssuesInMilestone(Repository repo, Milestone milestone)
        {
            var request = new RepositoryIssueRequest();
            request.Milestone = $"{milestone.Number}";
            return await GetIssues(repo, request);
        }

        public async Task<List<Issue>> GetIssuesWithLabel(Repository repo, string label)
        {
            var request = new RepositoryIssueRequest();
            request.Labels.Add(label);
            return await GetIssues(repo, request);
        }

        public async Task<Milestone> GetMilestone(Repository repo, string milestoneTitle)
        {
            var all = await _client.Issue.Milestone.GetAllForRepository(repo.Id);
            var comparer = StringComparer.OrdinalIgnoreCase;
            foreach (var item in all)
            {
                if (comparer.Equals(item.Title, milestoneTitle))
                {
                    return item;
                }
            }

            throw new Exception($"Unable to find milestone with title {milestoneTitle}");
        }

        public async Task<List<Issue>> GetIssues(Repository repo, RepositoryIssueRequest request, int pageSize = DefaultPageSize)
        {
            var list = new List<Issue>();
            var options = new ApiOptions()
            {
                PageCount = 1,
                StartPage = 1,
                PageSize = pageSize
            };

            var done = false;
            var issues = _client.Issue;
            do
            {
                var pageIssues = request != null
                    ? await issues.GetAllForRepository(repo.Id, request, options)
                    : await issues.GetAllForRepository(repo.Id, options);
                list.AddRange(pageIssues);
                if (pageIssues.Count < pageSize)
                {
                    done = true;
                }
                else
                {
                    options.StartPage++;
                }
            } while (!done);

            return list;
        }
    }
}
