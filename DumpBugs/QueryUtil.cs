using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DumpBugs
{
    internal sealed class QueryUtil
    {
        internal const int DefaultPageSize = 50;

        private readonly GitHubClient _client;

        internal QueryUtil(GitHubClient client)
        {
            _client = client;
        }

        internal async Task<List<Issue>> GetIssues(Repository repo, IssueQuery query)
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

        internal async Task<Milestone> GetMilestone(Repository repo, string title)
        {
            var all = await _client.Issue.Milestone.GetAllForRepository(repo.Id);
            var comparer = StringComparer.OrdinalIgnoreCase;
            foreach (var item in all)
            {
                if (comparer.Equals(item.Title, title))
                {
                    return item;
                }
            }

            throw new Exception($"Unable to find milestone with title {title}");
        }

        internal async Task<List<Issue>> GetIssues(Repository repo, RepositoryIssueRequest request, int pageSize = DefaultPageSize)
        {
            var list = new List<Issue>();
            var options = new ApiOptions()
            {
                PageCount = 1,
                StartPage = 0,
                PageSize = pageSize
            };

            var done = false;
            var issues = _client.Issue;
            do
            {
                var pageIssues = await issues.GetAllForRepository(repo.Id, request, options);
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
