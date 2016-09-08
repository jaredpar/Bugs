using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using TheBugs.Models;
using TheBugs.Utils;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using TheBugs.Storage;
using Microsoft.Azure;
using System.Threading;

namespace TheBugs.Controllers
{
    public class BugsController : Controller
    {
        private readonly CloudStorageAccount _storageAccount;

        public BugsController()
        {
            var connectionString = CloudConfigurationManager.GetSetting(Constants.StorageConnectionStringName);
            _storageAccount = CloudStorageAccount.Parse(connectionString);
        }

        public ActionResult Index()
        {
            return RedirectToAction(nameof(List));
        }

        public async Task<ActionResult> Graph(string assignee = null, string view = null, List<int> milestones = null)
        {
            var model = await RunCore(nameof(Graph), assignee, view, milestones);
            return View(model);
        }

        public async Task<ActionResult> List(string assignee = null, string view = null, List<int> milestones = null)
        {
            var model = await RunCore(nameof(List), assignee, view, milestones);
            return View(model);
        }

        private async Task<ListModel> RunCore(string actionName, string assignee = null, string view = null, List<int> milestones = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Case of parameter binding where milestone= is passed without a value.
            if (milestones != null && milestones.Count == 1 && milestones[0] == 0)
            {
                milestones.Clear();
            }

            var repoId = new RoachRepoId("dotnet", "roslyn");
            var queryUtil = new StorageQueryUtil(_storageAccount.CreateCloudTableClient());
            var foundIssues = await queryUtil.GetIssues(repoId, assignee, milestones, cancellationToken);
            var foundMilestones = await queryUtil.GetMilestones(repoId, cancellationToken);

            // var milestones = await queryUtil.GetMilestones(repoId);
            var query = new QueryModel(actionName, foundMilestones, view, assignee, milestones);
            var issues = SpecificFilterUtil.Filter(foundIssues, view);
            return new ListModel(query, issues.ToList());
        }
    }
}