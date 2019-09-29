using System.Web.Mvc;
using DashboardApplication;

namespace DashboardApplication.Controllers
{
    //page requests - each ActionResult corresponds to different page on website
    public class HomeController : Controller
    {
        public void getDatabases()
        {
            System.Diagnostics.Debug.WriteLine("Collecting data...");
            SQLGetter.GetSCSM();
            SQLGetter.GetSCCM();
            SQLGetter.GetEPO();
        }
        public ActionResult Index()
        {
            return View("Index");
        }

        public ActionResult Dashboard()
        {
            return View("Dashboard");
        }

        public ActionResult Collections()
        {
            return View("Collections");
        }

        public ActionResult Deployments()
        {
            return View("Deployments");
        }
    }
}