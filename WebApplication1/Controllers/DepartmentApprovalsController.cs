using AutoMapper;
using HZC.MyOrm;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class DepartmentApprovalsController : BaseController
    {
        private readonly ApprovalService _service;
        private readonly IMapper _mapper;
        private readonly MyDb _db;

        public DepartmentApprovalsController(IMapper mapper)
        {
            _db = new MyDb();
            _mapper = mapper;
            _service = new ApprovalService(mapper);
        }

        // 我的申请
        public IActionResult Index(int id = 1)
        {
            var pageIndex = id <= 0 ? 1 : id;
            var data = _service.DepartmentApprovals(CurrentUser.DepartmentId, id, out var total);
            ViewBag.PageIndex = pageIndex;
            ViewBag.RecordCount = total;
            return View(data);
        }

        // 待审批的申请
        public IActionResult WaitingApproved(int id = 1)
        {
            var pageIndex = id <= 0 ? 1 : id;
            var data = _service.DepartmentWaitingApproveApprovals(CurrentUser.DepartmentId, id, out var total);
            ViewBag.PageIndex = pageIndex;
            ViewBag.RecordCount = total;
            return View(data);
        }

        // 已审批
        public IActionResult WaitingConfirmed(int id = 1)
        {
            var pageIndex = id <= 0 ? 1 : id;
            var data = _service.DepartmentWaitingConfirmApprovals(CurrentUser.DepartmentId, id, out var total);
            ViewBag.PageIndex = pageIndex;
            ViewBag.RecordCount = total;
            return View(data);
        }

        #region 第一次审批
        public IActionResult Approve(int id)
        {
            var entity = _db.Load<Approval>(id);
            if (entity == null)
            {
                return NotFound();
            }
            ViewBag.Entity = entity;

            var model = new ApproveModel
            {
                Id = entity.Id,
                IsPass = entity.ApproveResult > 1,
                Remark = entity.ApproveRemark
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id, IFormCollection collection)
        {
            var entity = new ApproveModel();
            TryUpdateModelAsync(entity);

            if (ModelState.IsValid)
            {
                var result = _service.Approve(id, CurrentUser.No, CurrentUser.Name, entity.IsPass, entity.Remark);
                if (result > 0)
                {
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "审批失败");
            }
            var model = _db.Load<Approval>(id);
            ViewBag.Entity = model;
            return View(entity);
        }
        #endregion

        #region 二次审批

        public IActionResult Confirm(int id)
        {
            var entity = _db.Load<Approval>(id);

            if (entity == null)
            {
                return NotFound();
            }

            if (entity.ApproveStep != (int)ApproveStep.Audited)
            {
                return Content("仅财务审核过的记录可审核");
            }

            if (entity.ApplicantNo != CurrentUser.No)
            {
                return Content("申请人与当前账户不一致，禁止编辑");
            }

            ViewBag.Entity = entity;

            var model = new ApproveModel
            {
                Id = entity.Id,
                IsPass = entity.ApproveResult == 3,
                Remark = entity.ApproveRemark
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Confirm(int id, IFormCollection collection)
        {
            var entity = new ApproveModel();
            TryUpdateModelAsync(entity);

            if (ModelState.IsValid)
            {
                var result = _service.Confirm(id, entity.IsPass, entity.Remark, CurrentUser.No, CurrentUser.Name);
                if (result > 0)
                {
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError(string.Empty, "审核失败");
            }

            var model = _db.Load<Approval>(id);
            ViewBag.Entity = model;
            return View(entity);
        }
        #endregion
    }
}