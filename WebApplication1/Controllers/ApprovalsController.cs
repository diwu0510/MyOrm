using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HZC.MyOrm;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
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
        public IActionResult Index(int? id = 1)
        {
            var pageIndex = id.HasValue && id.Value > 0 ? id.Value : 1;
            var data = _db.Query<Approval>().Include(a => a.Department).ToPageList(pageIndex, 20, out var recordCount);
            ViewBag.PageIndex = pageIndex;
            ViewBag.RecordCount = recordCount;
            return View(data);
        }
        #endregion

        #region 创建
        public IActionResult Create(ApprovalDto dto)
        {
            dto.ApplicantNo = CurrentUser.No;
            dto.ApplicantName = CurrentUser.Name;
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

        #region 财务审核

        public IActionResult Audit(int id)
        {
            var entity = _db.Load<Approval>(id);

            if (entity == null)
            {
                return NotFound();
            }

            if (entity.ApproveStep == (int)ApproveStep.Close ||
                entity.ApproveStep == (int)ApproveStep.Confirmed ||
                entity.ApproveStep != (int)ApproveStep.Approved)
            {
                return Content("已关闭、已完成或未审批的申请，禁止编辑");
            }

            if (entity.ApplicantNo != CurrentUser.No)
            {
                return Content("申请人与当前账户不一致，禁止编辑");
            }

            var dto = _mapper.Map<AuditModel>(entity);
            return View(dto);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Audit(int id, IFormCollection collection)
        {
            var dto = new AuditModel();
            TryUpdateModelAsync(dto);

            if (ModelState.IsValid)
            {
                var result = _service.Audit(id, dto.AuditProfit, dto.ActualServiceAmount, CurrentUser.No,
                    CurrentUser.Name);
                if (result > 0)
                {
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "审核失败");
            }

            return View(dto);
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

            var dto = _mapper.Map<AuditModel>(entity);
            return View(dto);
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

            return View(entity);
        }
        #endregion
    }
}