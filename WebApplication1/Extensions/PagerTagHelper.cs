using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace WebApplication1.Extensions
{
    public class PagerTagHelper : TagHelper
    {
        /// <summary>
        /// 数据总数
        /// </summary>
        public int Total { get; set; } = 0;

        /// <summary>
        /// 当前页码
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 每页记录数
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// 当前页路由
        /// </summary>
        public string RouteUrl { get; set; }

        /// <summary>
        /// 当前页的查询条件
        /// </summary>
        public string Query { get; set; }

        private string SetQueryString()
        {
            var result = new List<string>();
            if (!string.IsNullOrWhiteSpace(Query))
            {
                if (Query.StartsWith("?"))
                {
                    Query = Query.Remove(0, 1);
                }

                string[] paramList = Query.Split('&');
                foreach (var param in paramList)
                {
                    var paramName = param.Trim().ToLower();
                    if (!paramName.StartsWith("pageindex=") && !paramName.StartsWith("pagesize="))
                    {
                        result.Add(param);
                    }
                }
                // 用LINQ遍历
                // result = paramList.Where(p => !p.ToLower().StartsWith("pageindex=") && !p.ToLower().StartsWith("pagesize=")).ToList();
            }
            result.Add("pageIndex={0}");
            result.Add("pageSize=" + PageSize.ToString());
            return "?" + string.Join('&', result);
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.Add("class", "my-pager");

            if (PageSize <= 0) { PageSize = 20; }
            if (PageIndex <= 0) { PageIndex = 1; }
            if (Total <= 0) { return; }

            //总页数
            var totalPage = Total / PageSize + (Total % PageSize > 0 ? 1 : 0);
            if (totalPage <= 0) { return; }

            Query = SetQueryString();

            //构造分页样式
            var sbPage = new StringBuilder(string.Empty);

            sbPage.Append("<ul class=\"pagination\">");
            sbPage.AppendFormat("<li><a href=\"{0}{1}\">首页</a></li>",
                RouteUrl,
                string.Format(Query, 1)
            );

            // 计算显示的页码
            int start = 1;
            int end = totalPage;
            bool hasStart = false;
            bool hasEnd = false;

            if (totalPage > 10)
            {
                if (PageIndex > 5)
                {
                    start = PageIndex - 4;
                    hasStart = true;
                }

                if (start + 9 < totalPage)
                {
                    end = start + 9;
                    hasEnd = true;
                }
                else
                {
                    end = totalPage;
                    start = totalPage - 9;
                }
            }

            if (hasStart)
            {
                sbPage.AppendFormat("<li><a href=\"{0}{1}\">...</a></li>",
                    RouteUrl,
                    string.Format(Query, start - 1)
                );
            }

            for (int i = start; i <= end; i++)
            {
                sbPage.AppendFormat("<li {1}><a href=\"{2}{3}\">{0}</a></li>",
                    i,
                    i == PageIndex ? "class=\"active\"" : "",
                    RouteUrl,
                    string.Format(Query, i)
                );
            }

            if (hasEnd)
            {
                sbPage.AppendFormat("<li><a href=\"{0}{1}\">...</a></li>",
                    RouteUrl,
                    string.Format(Query, end + 1)
                );
            }

            sbPage.Append("<li>");
            sbPage.AppendFormat("<a href=\"{0}{1}\">",
                                RouteUrl,
                                string.Format(Query, totalPage));
            sbPage.Append("尾页");
            sbPage.Append("</a>");
            sbPage.Append("</li>");
            sbPage.Append("</ul>");

            output.Content.SetHtmlContent(sbPage.ToString());
        }
    }
}
