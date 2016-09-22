using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TheBugs.GitHub
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

        public async Task<IReadOnlyList<Milestone>> GetMilestones(RoachRepoId repoId)
        {
            var request = new MilestoneRequest()
            {
                State = ItemStateFilter.All
            };

            return await _client.Issue.Milestone.GetAllForRepository(repoId.Owner, repoId.Name, request);
        }

        public async Task<IEnumerable<Milestone>> GetMilestones(RoachRepoId repoId, IEnumerable<int> milestoneNumbers)
        {
            var all = await GetMilestones(repoId);
            return all.Where(x => milestoneNumbers.Contains(x.Number));
        }

        public async Task<List<Issue>> GetIssuesInMilestone(RoachMilestone milestone)
        {
            var request = new RepositoryIssueRequest();
            request.Milestone = $"{milestone.Number}";
            return await GetIssues(milestone.RepoId, request);
        }

        public async Task<IEnumerable<Milestone>> GetMilestones(RoachRepoId repoId, IEnumerable<string> milestoneTitles)
        {
            var all = await GetMilestones(repoId);
            return all.Where(x => milestoneTitles.Contains(x.Title));
        }

        public async Task<IReadOnlyList<Issue>> GetIssuesChangedSince(RoachRepoId repoId, DateTimeOffset since)
        {
            var request = new RepositoryIssueRequest()
            {
                Since = since,
                State = ItemStateFilter.All,
            };
            return await _client.Issue.GetAllForRepository(repoId.Owner, repoId.Name, request);
        }


        public async Task<List<Issue>> GetIssues(RoachRepoId repo, RepositoryIssueRequest request, int pageSize = DefaultPageSize)
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
                    ? await issues.GetAllForRepository(repo.Owner, repo.Name, request, options)
                    : await issues.GetAllForRepository(repo.Owner, repo.Name, options);
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
