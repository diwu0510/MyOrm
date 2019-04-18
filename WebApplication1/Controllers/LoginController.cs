using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HZC.MyOrm;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class LoginController : Controller
    {
        private readonly MyDb _db = new MyDb();

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login(string name)
        {
            var dto = new UserDto();
            if (name == "admin")
            {
                dto.No = "admin";
                dto.Name = "admin";
                dto.Role = "admin";
                dto.DepartmentId = 1;
            }

            var user = _db.Load<AppUser>(u => u.Name == name && u.IsDelete == false);
            if (user == null)
            {
                return Content("用户不存在");
            }
            else
            {
                dto.No = user.No;
                dto.Name = user.Name;
                dto.Role = user.IsMaster ? "master" : "user";
                dto.DepartmentId = user.DepartmentId;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, dto.No),
                new Claim(ClaimTypes.Name, dto.Name),
                new Claim(ClaimTypes.Role, dto.Role),
                new Claim("Department", dto.DepartmentId.ToString())
            };

            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));

            return RedirectToAction("Index", "Home");
        }
    }
}