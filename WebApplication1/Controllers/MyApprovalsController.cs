using AutoMapper;
using HZC.MyOrm;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Extensions;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [WeixinUser("user")]
    public class MyApprovalsController : BaseController
    {
        private readonly ApprovalService _service;
        private readonly IMapper _mapper;
        private readonly MyDb _db;

        public MyApprovalsController(IMapper mapper)
        {
            _db = new MyDb();
            _mapper = mapper;
            _service = new ApprovalService(mapper);

            var user = User;
        }

        // 我的申请
        public IActionResult Index(int id = 1)
        {
            var user = User;
            var pageIndex = id <= 0 ? 1 : id;
            var data = _service.MyApprovals(CurrentUser.No, id, out var total);
            
            ViewBag.PageIndex = pageIndex;
            ViewBag.RecordCount = total;
            return View(data);
        }

        // 待审批的申请
        public IActionResult WaitingApproved(int id = 1)
        {
            var pageIndex = id <= 0 ? 1 : id;
            var data = _service.MyWaitingApproveApprovals(CurrentUser.No, id, out var total);
            ViewBag.PageIndex = pageIndex;
            ViewBag.RecordCount = total;
            return View(data);
        }

        public IActionResult Approved(int id = 1)
        {
            var pageIndex = id <= 0 ? 1 : id;
            var data = _service.MyApprovedApprovals(CurrentUser.No, id, out var total);
            ViewBag.PageIndex = pageIndex;
            ViewBag.RecordCount = total;
            return View(data);
        }

        public IActionResult Audited(int id = 1)
        {
            var pageIndex = id <= 0 ? 1 : id;
            var data = _service.MyAuditedApprovals(CurrentUser.No, id, out var total);
            ViewBag.PageIndex = pageIndex;
            ViewBag.RecordCount = total;
            return View(data);
        }

        public IActionResult Confirmed(int id = 1)
        {
            var pageIndex = id <= 0 ? 1 : id;
            var data = _service.MyConfirmedApprovals(CurrentUser.No, id, out var total);
            ViewBag.PageIndex = pageIndex;
            ViewBag.RecordCount = total;
            return View(data);
        }

        #region 创建
        public IActionResult Create(ApprovalDto dto)
        {
            dto.ApplicantNo = CurrentUser.No;
            dto.ApplicantName = CurrentUser.Name;
            dto.DepartmentId = CurrentUser.DepartmentId;
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(IFormCollection collection)
        {
            var entity = new ApprovalDto();
            TryUpdateModelAsync(entity);
            if (ModelState.IsValid)
            {
                var id = _service.Create(entity, CurrentUser.No, CurrentUser.Name, CurrentUser.DepartmentId);
                if (id > 0)
                {
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "创建失败");
            }
            return View(entity);
        }
        #endregion

        #region 完善资料

        public IActionResult Supplement(int id)
        {
            var entity = _db.Load<Approval>(id);

            if (entity == null)
            {
                return NotFound();
            }

            if (entity.ApproveStep == (int)ApproveStep.Close ||
                entity.ApproveStep == (int)ApproveStep.Confirmed)
            {
                return Content("申请已关闭或已完成，禁止编辑");
            }

            if (entity.ApplicantNo != CurrentUser.No)
            {
                return Content("申请人与当前账户不一致，禁止编辑");
            }

            ViewBag.Entity = entity;

            var dto = _mapper.Map<SupplementDto>(entity);
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Supplement(int id, IFormCollection collection)
        {
            var dto = new SupplementDto();
            TryUpdateModelAsync(dto);

            if (ModelState.IsValid)
            {
                var result = _service.Supplement(id, dto);
                if (result > 0)
                {
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "更新失败");
            }

            return View(dto);
        }
        #endregion

        #region 详情

        public IActionResult Details(int id)
        {
            var entity = _db.Load<Approval>(id);
            return View(entity);
        }
        #endregion

        #region 修改
        public IActionResult Edit(int id)
        {
            var entity = _db.Load<Approval>(id);
            if (entity == null)
            {
                return NotFound();
            }
            if (entity.ApplicantNo != CurrentUser.No)
            {
                return Content("无权编辑此记录");
            }
            if (entity.ApproveStep != 0)
            {
                return Content("已审批申请禁止编辑");
            }
            var dto = _mapper.Map<ApprovalDto>(entity);
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, IFormCollection collection)
        {
            var entity = new ApprovalDto();
            TryUpdateModelAsync(entity);

            if (ModelState.IsValid)
            {
                if (entity.Id != id)
                {
                    ModelState.AddModelError(string.Empty, "id参数无效");
                }
                else if (entity.ApplicantNo != CurrentUser.No)
                {
                    ModelState.AddModelError(string.Empty, "无权编辑此记录");
                }
                else
                {
                    var result = _service.Update(id, entity);
                    if (result > 0)
                    {
                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError(string.Empty, "更新失败");
                }
            }

            return View(entity);
        }
        #endregion
    }
}