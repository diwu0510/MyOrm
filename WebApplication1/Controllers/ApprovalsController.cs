using AutoMapper;
using HZC.MyOrm;
using HZC.MyOrm.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "admin")]
    public class ApprovalsController : BaseController
    {
        private readonly MyDb _db;

        private readonly ApprovalService _service;

        private readonly IMapper _mapper;

        public ApprovalsController(IMapper mapper)
        {
            _db = MyDb.New();
            _mapper = mapper;
            _service = new ApprovalService(_mapper);
        }

        #region 首页
        public IActionResult Index(int id, ApprovalSearchParameter search)
        {
            var pageIndex = id > 0 ? id : 1;

            var where = LinqExtensions.True<Approval>();
            if (search.Department > 0)
            {
                where = where.And(a => a.DepartmentId == search.Department);
            }

            if (!string.IsNullOrWhiteSpace(search.Creator))
            {
                where = where.And(a => a.ApplicantNo == search.Creator);
            }

            if (search.CreateAtStart.HasValue)
            {
                where = where.And(a => a.CreateAt >= search.CreateAtStart.Value);
            }

            if (search.CreateAtEnd.HasValue)
            {
                where = where.And(a => a.CreateAt <= search.CreateAtEnd.Value);
            }

            if (search.Step > 0)
            {
                where = where.And(a => a.ApproveStep == search.Step);
            }

            if (search.Result > 0)
            {
                where = where.And(a => a.ApproveResult == search.Result);
            }

            if (!string.IsNullOrWhiteSpace(search.Approver))
            {
                where = where.And(a => a.ApproverNo == search.Approver || a.ConfirmNo == search.Approver);
            }

            var query = _db.Query<Approval>().Include(a => a.Department);
            var data = query.Where(where).ToPageList(pageIndex, 20, out var recordCount);

            var result = _mapper.Map<ApprovalResult>(search);

            result.PageIndex = pageIndex;
            result.RecordCount = recordCount;
            result.PageCount = recordCount / 20 + (recordCount % 20 > 0 ? 1 : 0);
            result.Data = data;

            ViewBag.Departments = new SelectList(_db.Fetch<Department>(), "Id", "Name");
            return View(result);
        }
        #endregion
        

        #region 详情

        public IActionResult Details(int id)
        {
            var entity = _db.Load<Approval>(id);
            return View(entity);
        }
        #endregion
    }
}