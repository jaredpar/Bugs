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

namespace TheBugs.Controllers
{
    public class BugsController : Controller
    {
        public BugsController()
        {

        }

        public ActionResult Index()
        {
            var storage = Storage.GetOrCreate(Server);
            return View();
        }

        public ActionResult Graph(string filter = null)
        {
            var storage = Storage.GetOrCreate(Server);
            return View(Storage.Filter(storage.Issues, filter));
        }

        public ActionResult Assigned(string assignee = null, string filter = null)
        {
            var storage = Storage.GetOrCreate(Server);
            var issues = Storage.Filter(storage.Issues, filter);

            if (!string.IsNullOrEmpty(assignee))
            {
                issues = issues.Where(x => x.Assignee == assignee);
            }

            var model = new AssignedBugs(assignee, filter, issues.ToList());
            return View(model);
        }
    }
}