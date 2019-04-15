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

        public IActionResult Index(int? id = 1)
        {
            var pageIndex = id.HasValue && id.Value > 0 ? id.Value : 1;
            var data = _db.Query<Approval>().Include(a => a.Department).ToPageList(pageIndex, 20, out var recordCount);
            return View(data);
        }

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
            if(ModelState.IsValid)
            {
                var id = _service.Create(entity, CurrentUser.No, CurrentUser.Name, CurrentUser.DepartmentId);
                if(id > 0)
                {
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "创建失败");
            }
            return View(entity);
        }

        public IActionResult Edit(int id)
        {
            var entity = _db.Load<Approval>(id);
            if(entity == null)
            {
                return NotFound();
            }
            if(entity.ApplicantNo != CurrentUser.No)
            {
                return Content("无权编辑此记录");
            }
            if(entity.ApproveStep != 0)
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

            if(ModelState.IsValid)
            {
                if(entity.Id != id)
                {
                    ModelState.AddModelError(string.Empty, "id参数无效");
                }
                else if(entity.ApplicantNo != CurrentUser.No)
                {
                    ModelState.AddModelError(string.Empty, "无权编辑此记录");
                }
                else
                {
                    var result = _service.Update(id, entity);
                    if(result > 0)
                    {
                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError(string.Empty, "更新失败");
                }
            }

            return View(entity);
        }

        public IActionResult Approve(int id)
        {
            var entity = _db.Load<Approval>(id);
            if(entity == null)
            {
                return NotFound();
            }
            ViewBag.Entity = entity;
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id, IFormCollection collection)
        {
            var entity = new ApproveModel();
            TryUpdateModelAsync(entity);

            if(ModelState.IsValid)
            {
                var result = _service.Approve(id, CurrentUser.No, CurrentUser.Name, entity.Result, entity.Remark);
                if(result > 0)
                {
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "审批失败");
            }

            return View(entity);
        }
    }
}