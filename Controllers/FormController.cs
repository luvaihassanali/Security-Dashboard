using System.Web.Mvc;
namespace DashboardApplication.Controllers
{
    //implementation of POST requests to render search queries + dashboard
    public class FormController : Controller
    {
        [HttpPost] //Search request
        public string FormOne(Models.FormData formData)
        {
            System.Diagnostics.Debug.WriteLine("searching for: " + formData.TextBoxStringData);
            SQLGetter.Search(formData.TextBoxStringData);
            string ADInfo = ADGetter.parseData(formData.TextBoxStringData);
            return "<br><strong><span style='color:#2c86d4'>Active Directory</span></strong>" + ADInfo + "<strong><span style='color:#2c86d4'>System Center Configuration Manager</span></strong>" + SQLGetter.SCCMTableString + "<strong><span style='color:#2c86d4'>McAfee ePolicy Orchestrator</span></strong>" + SQLGetter.EPOTableString + "<strong><span style='color:#2c86d4'>System Center Service Manager</span></strong>" + SQLGetter.SCSMTableString;
        }

        [HttpPost] //Dashboard data request
        public string FormTwo(Models.FormData formData)
        {
            if (formData.TextBoxStringData == "dashboards")
            {
                System.Diagnostics.Debug.WriteLine("rendering dashboard");
                return SQLGetter.GetDashData();
            }
            if (formData.TextBoxStringData == "spreadsheets")
            {
                System.Diagnostics.Debug.WriteLine("rendering collections");
                return SQLGetter.GetCollectionData();
            }
            if(formData.TextBoxStringData == "deployments")
            {
                System.Diagnostics.Debug.WriteLine("rendering deployments");
                return SQLGetter.GetDeploymentData();
            }
            if(formData.TextBoxStringData == "reloadData")
            {
                SQLGetter.GetSCCM();
                SQLGetter.GetEPO();
                SQLGetter.GetSCSM();
                return "success";  
            }
            return "";
        }
    }

}