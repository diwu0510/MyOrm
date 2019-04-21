using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace WebApplication1.Extensions
{
    public class WeixinUserAttribute : Attribute, IAuthorizationFilter
    {
        public string Role { get; set; }

        public WeixinUserAttribute(string role)
        {
            Role = role;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                var url = "http://" + context.HttpContext.Request.Host.Host + context.HttpContext.Request.Path;
                RedirectResult result = new RedirectResult("/OAuth2/Index?returnUrl=" + url);
                context.Result = result;
            }
            else if(!context.HttpContext.User.IsInRole(Role))
            {
                var url = "http://" + context.HttpContext.Request.Host.Host + context.HttpContext.Request.Path;
                RedirectToActionResult result = new RedirectToActionResult("Index", "Error", new
                {
                    Code = 401,
                    Title = "无权访问",
                    Message = "您已登录，但无权使用此功能"
                });
                context.Result = result;
            }
        }
    }
}
