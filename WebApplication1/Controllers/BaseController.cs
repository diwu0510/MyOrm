using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    public class BaseController : Controller
    {
        protected AppUser CurrentUser
        {
            get
            {
                return new AppUser
                {
                    Id = 1,
                    No = "hanzuochao",
                    Name = "韩作超",
                    DepartmentId = 1
                };
            }
        }
    }
}
