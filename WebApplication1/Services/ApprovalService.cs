using AutoMapper;
using HZC.MyOrm;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class ApprovalService 
    {
        private readonly MyDb _db = MyDb.New();

        private readonly IMapper _mapper;

        public ApprovalService(IMapper mapper)
        {
            _mapper = mapper;
        }

        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="userNo"></param>
        /// <param name="userName"></param>
        /// <param name="departmentId"></param>
        /// <returns></returns>
        public int Create(ApprovalDto dto, string userNo, string userName, int departmentId)
        {
            var entity = _mapper.Map<Approval>(dto);
            entity.ApplicantNo = userNo;
            entity.ApplicantName = userName;
            entity.DepartmentId = departmentId;
            entity.ApproveStep = 0;
            entity.ApproveResult = 0;
            entity.CreateAt = DateTime.Now;

            return _db.Insert(entity);
        }

        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public int Update(int id, ApprovalDto dto)
        {
            if (dto.Id != id)
            {
                return 0;
            }

            var entity = _db.Load<Approval>(id);
            if (entity == null)
            {
                return 0;
            }

            entity.CustomerName = dto.CustomerName;
            entity.CustomerJob = dto.CustomerJob;
            entity.CustomerUnit = dto.CustomerUnit;
            entity.ContactNumber = dto.ContactNumber;
            entity.ProjectName = dto.ProjectName;
            entity.ExpectedClosingCost = dto.ExpectedClosingCost;
            entity.ExpectedClosingDate = dto.ExpectedClosingDate;
            entity.ActualClosingProfit = dto.ExpectedClosingProfit;
            entity.AppliedAmount = dto.AppliedAmount;
            entity.AppliedReason = dto.AppliedReason;

            return _db.Update(entity);
        }

        /// <summary>
        /// 第一次审批
        /// </summary>
        /// <param name="id"></param>
        /// <param name="approverNo"></param>
        /// <param name="approverName"></param>
        /// <param name="isPass"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public int Approve(int id, string approverNo, string approverName, bool isPass, string remark)
        {
            var entity = _db.Load<Approval>(id);
            if (entity == null)
            {
                return 0;
            }

            entity.ApproverNo = approverNo;
            entity.ApproverName = approverName;
            entity.ApproveRemark = remark;
            entity.ApproveResult = isPass ? (int)ApproveResult.ApprovePass : (int)ApproveResult.ApproveRefuse;
            entity.ApproveAt = DateTime.Now;
            entity.ApproveStep = (int)ApproveStep.Approved;

            return _db.Update(entity);
        }

        /// <summary>
        /// 完善资料
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public int Supplement(int id, SupplementDto dto)
        {
            var entity = _db.Load<Approval>(id);
            if (entity == null)
            {
                return 0;
            }

            entity.OrderNo = dto.OrderNo;
            entity.IsHasInvoice = dto.IsHasInvoice;
            entity.TaxPoint = dto.TaxPoint;
            entity.PaymentInfo = dto.PaymentInfo;
            entity.ActualClosingAmount = dto.ActualClosingAmount;
            entity.ActualClosingProfit = dto.ActualClosingProfit;
            entity.Collections = dto.Collections;
            entity.CompleteAt = dto.CompleteAt;

            return _db.Update(entity);
        }

        /// <summary>
        /// 财务审核
        /// </summary>
        /// <param name="id"></param>
        /// <param name="profit"></param>
        /// <param name="serviceAmount"></param>
        /// <param name="userNo"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public int Audit(int id, decimal profit, decimal serviceAmount, string userNo, string userName)
        {
            var entity = _db.Load<Approval>(id);
            if (entity == null)
            {
                return 0;
            }

            entity.AuditProfit = profit;
            entity.ActualServiceAmount = serviceAmount;
            entity.AuditorNo = userNo;
            entity.AuditorName = userName;
            entity.AuditAt = DateTime.Now;
            entity.ApproveStep = (int) ApproveStep.Audited;

            return _db.Update(entity);
        }

        /// <summary>
        /// 确认审批
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isPass"></param>
        /// <param name="remark"></param>
        /// <param name="userNo"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public int Confirm(int id, bool isPass, string remark, string userNo, string userName)
        {
            var entity = _db.Load<Approval>(id);
            if (entity == null)
            {
                return 0;
            }

            entity.ApproveResult = isPass ? (int) ApproveResult.ConfirmPass : (int) ApproveResult.ConfirmRefuse;
            entity.ConfirmRemark = remark;
            entity.ConfirmNo = userNo;
            entity.ConfirmName = userName;
            entity.ConfirmAt = DateTime.Now;
            entity.ApproveStep = (int) ApproveStep.Confirmed;

            return _db.Update(entity);
        }

        #region 我的申请
        // 我的申请
        public List<Approval> MyApprovals(string userNo, int pageIndex, out int total)
        {
            pageIndex = pageIndex <= 0 ? 1 : pageIndex;
            var list = _db.Query<Approval>()
                .Where(a => a.ApplicantNo == userNo)
                .ToPageList(pageIndex, 20, out var count);
            total = count;
            return list;
        }

        // 待审批的申请
        public List<Approval> MyWaitingApproveApprovals(string userNo, int pageIndex, out int total)
        {
            pageIndex = pageIndex <= 0 ? 1 : pageIndex;
            var list = _db.Query<Approval>()
                .Where(a => a.ApplicantNo == userNo && a.ApproveStep == 0)
                .ToPageList(pageIndex, 20, out var count);
            total = count;
            return list;
        }

        // 已审批的申请
        public List<Approval> MyApprovedApprovals(string userNo, int pageIndex, out int total)
        {
            pageIndex = pageIndex <= 0 ? 1 : pageIndex;
            var list = _db.Query<Approval>()
                .Where(a => a.ApplicantNo == userNo && a.ApproveStep == (int) ApproveStep.Approved)
                .ToPageList(pageIndex, 20, out var count);
            total = count;
            return list;
        }

        // 已审核的申请
        public List<Approval> MyAuditedApprovals(string userNo, int pageIndex, out int total)
        {
            pageIndex = pageIndex <= 0 ? 1 : pageIndex;
            var list = _db.Query<Approval>()
                .Where(a => a.ApplicantNo == userNo && a.ApproveStep == (int)ApproveStep.Audited)
                .ToPageList(pageIndex, 20, out var count);
            total = count;
            return list;
        }

        // 已确认的申请
        public List<Approval> MyConfirmedApprovals(string userNo, int pageIndex, out int total)
        {
            pageIndex = pageIndex <= 0 ? 1 : pageIndex;
            var list = _db.Query<Approval>()
                .Where(a => a.ApplicantNo == userNo && a.ApproveStep == (int)ApproveStep.Confirmed)
                .ToPageList(pageIndex, 20, out var count);
            total = count;
            return list;
        }
        #endregion

        #region 我审批的申请

        // 所有申请
        public List<Approval> DepartmentApprovals(int departmentId, int pageIndex, out int total)
        {
            pageIndex = pageIndex <= 0 ? 1 : pageIndex;
            var list = _db.Query<Approval>()
                .Where(a => a.DepartmentId == departmentId)
                .ToPageList(pageIndex, 20, out var count);
            total = count;
            return list;
        }

        // 要我审批的
        public List<Approval> DepartmentWaitingApproveApprovals(int departmentId, int pageIndex, out int total)
        {
            pageIndex = pageIndex <= 0 ? 1 : pageIndex;
            var list = _db.Query<Approval>()
                .Where(a => a.DepartmentId == departmentId && a.ApproveStep == 0)
                .ToPageList(pageIndex, 20, out var count);
            total = count;
            return list;
        }

        // 要我确认的
        public List<Approval> DepartmentWaitingConfirmApprovals(int departmentId, int pageIndex, out int total)
        {
            pageIndex = pageIndex <= 0 ? 1 : pageIndex;
            var list = _db.Query<Approval>()
                .Where(a => a.DepartmentId == departmentId && a.ApproveStep == (int)ApproveStep.Audited)
                .ToPageList(pageIndex, 20, out var count);
            total = count;
            return list;
        }
        #endregion

        // 需要财务审核的
        public List<Approval> WaitingAuditApprovals(int pageIndex, out int total)
        {
            pageIndex = pageIndex <= 0 ? 1 : pageIndex;
            var list = _db.Query<Approval>()
                .Where(a => a.ApproveStep == (int)ApproveStep.Approved)
                .ToPageList(pageIndex, 20, out var count);
            total = count;
            return list;
        }
    }
}
