using System.IO;
using System.Linq;
using System.Web.Mvc;
using DevExtremeMvcApp2.Models;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;


[Authorize]
public class ShapeController : Controller
{
   


   
    public ActionResult Index()
    {
        return View();
    }



}
