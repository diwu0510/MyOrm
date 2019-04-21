using HZC.MyOrm;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Senparc.CO2NET.Extensions;
using Senparc.Weixin.Work.AdvancedAPIs;
using Senparc.Weixin.Work.Containers;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using WebApplication1;
using WebApplication1.Config;
using WebApplication1.Models;

namespace Zodo.Assets.Website.Controllers
{
    public class OAuth2Controller : Controller
    {
        private readonly string _corpId = WeixinWorkOptions.CorpId;
        private readonly string _secret = WeixinWorkOptions.Secret;
        private readonly string _agentId = WeixinWorkOptions.AgentId;

        public IActionResult Index(string returnUrl)
        {
            var redirectUrl = "http://" + HttpContext.Request.Host.Host + "/OAuth2/UserInfoCallback?returnUrl=" + returnUrl.UrlEncode();
            var url = OAuth2Api.GetCode(_corpId, redirectUrl, "", _agentId);
            return Redirect(url);
        }

        public IActionResult UserInfoCallback(string code, string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return RedirectToAction("Index", "Error", new { title = "无效访问", message = "您的请求参数不合法，请从正规渠道访问此功能。" });
            }

            try
            {
                var token = AccessTokenContainer.TryGetToken(_corpId, _secret);
                if (string.IsNullOrWhiteSpace(token))
                {
                    //_log.Error("获取ACCESSTOKEN详情失败：AccessTokenContainer.TryGetToken()获取失败");
                    return RedirectToAction("Index", "Error", new { title = "访问失败", message = "从微信服务端请求数据失败，请稍候再试。" });
                }
                
                var user = OAuth2Api.GetUserId(token, code);
                if (user.errcode != Senparc.Weixin.ReturnCode_Work.请求成功)
                {
                    //_log.Error("获取用户ID失败：" + user.errmsg);
                    return RedirectToAction("Index", "Error", new { title = "加载失败", message = "从微信服务端获取用户信息失败，请联系管理员或稍候再试" });
                }

                if (string.IsNullOrWhiteSpace(user.UserId))
                {
                    //_log.Error("获取用户ID失败，接口调用成功，但USERID为空：" + JsonConvert.SerializeObject(user));
                    return RedirectToAction("Index", "Error", new { title = "拒绝访问", message = "仅限企业微信内部员工使用，未能获取到您的数据，请联系管理员" });
                }
                else
                {
                    var db = MyDb.New();
                    var appUser = db.Load<AppUser>(a => a.No == user.UserId);

                    if (appUser == null)
                    {
                        return RedirectToAction("Index", "Error", new { title = "访问失败", message = "无权访问，请联系管理员" });
                    }
                    else
                    {
                        var dto = new UserDto {No = appUser.No, Name = appUser.Name, DepartmentId = appUser.DepartmentId };
                        if (appUser.IsFinance)
                        {
                            dto.Role = "audit";
                        }
                        else
                        {
                            dto.Role = appUser.IsMaster ? "master" : "user";
                        }

                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, dto.No),
                            new Claim(ClaimTypes.Name, dto.Name),
                            new Claim(ClaimTypes.Role, dto.Role),
                            new Claim("Department", dto.DepartmentId.ToString())
                        };

                        HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                        HttpContext.SignInAsync(
                            new ClaimsPrincipal(
                                new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme)
                            )
                        );

                        return Redirect(returnUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                //_log.Error(ex);
                return RedirectToAction("Index", "Error", new { title = "访问失败", message = "系统错误：" + ex.Message + "，请联系管理员" });
            }
        }

        #region 错误页面
        public IActionResult Error(string title, string message)
        {
            ViewData["Title"] = string.IsNullOrWhiteSpace(title) ? "操作失败" : title;
            ViewData["Message"] = message;
            return View();
        }
        #endregion
    }
}