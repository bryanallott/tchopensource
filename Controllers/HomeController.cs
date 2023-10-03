using ConvergencyBloc.AzureBlob;
using System;
using System.Web.Mvc;
using TchOpenSource.Models;

namespace TchOpenSource.Controllers
{
    public class HomeController : Controller
    {
        protected CloudFileHandler GetCloudFileHandler()
        {
            return new CloudFileHandler(OpenSourceConfig.CloudStorageConnectionString, OpenSourceConfig.CloudStorageContainerReference);
        }
        public ActionResult Verify(Guid id)
        {
            ConvergencyCloudFile cloud = GetCloudFileHandler()
                                            .GetFile($"stamped/{id}");
            return Redirect(cloud.URL);
        }
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult About()
        {
            ViewBag.Message = "The Convergency Hub Pdf Utils.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "info@theconvergency.co.za";
            return View();
        }
    }
}