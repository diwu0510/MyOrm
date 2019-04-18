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
    public class AuditApprovalsController : BaseController
    {
        private readonly ApprovalService _service;
        private readonly IMapper _mapper;
        private readonly MyDb _db;

        public AuditApprovalsController(IMapper mapper)
        {
            _db = new MyDb();
            _mapper = mapper;
            _service = new ApprovalService(mapper);
        }

        public IActionResult Index(int id = 1)
        {
            var pageIndex = id <= 0 ? 1 : id;
            var data = _service.WaitingAuditApprovals(id, out var total);
            ViewBag.PageIndex = pageIndex;
            ViewBag.RecordCount = total;
            return View(data);
        }

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

            ViewBag.Entity = entity;

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
            var entity = _db.Load<Approval>(id);
            ViewBag.Entity = entity;
            return View(dto);
        }

        #endregion
    }
}