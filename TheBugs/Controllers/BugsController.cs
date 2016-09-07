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

namespace TheBugs.Controllers
{
    public class BugsController : Controller
    {
        public BugsController()
        {

        }

        public ActionResult Index()
        {
            return RedirectToAction(nameof(List));
        }

        public ActionResult Graph(string assignee = null, string view = null, List<int> milestones = null)
        {
            return RunCore(nameof(Graph), assignee, view, milestones);
        }

        public ActionResult List(string assignee = null, string view = null, List<int> milestones = null)
        {
            return RunCore(nameof(List), assignee, view, milestones);
        }

        private ActionResult RunCore(string actionName, string assignee = null, string view = null, List<int> milestones = null)
        {
            // Case of parameter binding where milestone= is passed without a value.
            if (milestones != null && milestones.Count == 1 && milestones[0] == 0)
            {
                milestones.Clear();
            }

            var storage = DataStorage.GetOrCreate(Server);

            var query = QueryModel.Create(storage, actionName: actionName, assignee: assignee, view: view, milestones: milestones);
            var issues = storage.Filter(assignee, view, query.Milestones);
            var model = new ListModel(query, issues.ToList());
            return View(model);
        }
    }
}