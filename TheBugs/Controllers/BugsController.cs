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

        public ActionResult Graph()
        {
            var storage = Storage.GetOrCreate(Server);
            return View(storage.Issues);
        }
    }
}