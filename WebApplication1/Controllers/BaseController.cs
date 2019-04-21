using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class BaseController : Controller
    {
        public UserDto CurrentUser
        {
            get
            {
                if (User != null && User.Identity.IsAuthenticated)
                {
                    return new UserDto
                    {
                        No = User.FindFirst(ClaimTypes.NameIdentifier).Value,
                        Name = User.FindFirst(ClaimTypes.Name).Value,
                        DepartmentId = int.Parse(User.FindFirst("Department").Value)
                    };
                }

                return null;
            }
        }
    }
}
