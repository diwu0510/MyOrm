using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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
