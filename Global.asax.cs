using Microsoft.ApplicationInsights.Extensibility.Implementation;
using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace DashboardApplication
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            TelemetryDebugWriter.IsTracingDisabled = true;
        }

       //reload databases at each new session or refresh
       void Session_Start(object sender, EventArgs e)
       {
            SQLGetter.GetSCSM();
            SQLGetter.GetSCCM();
            SQLGetter.GetEPO();
        }
    }
}
