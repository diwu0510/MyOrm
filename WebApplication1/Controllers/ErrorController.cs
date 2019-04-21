using System.Net;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Index(ErrorPageViewModel model)
        {
            return View(model);
        }

        public IActionResult Deny(string returnUrl = "")
        {
            var model = new ErrorPageViewModel(401, "无权访问", "您尚未登录");
            return View("Index", model);
        }
    }
}