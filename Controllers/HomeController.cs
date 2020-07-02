using System.Web.Mvc;

namespace TchOpenSource.Controllers
{
    public class HomeController : Controller
    {
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