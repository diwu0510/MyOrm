using AutoMapper;
using HZC.MyOrm;
using System;
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

        public int Approve(int id, string approverNo, string approverName, ApproveResult result, string remark)
        {
            var entity = _db.Load<Approval>(id);
            if (entity == null)
            {
                return 0;
            }

            entity.ApproverNo = approverNo;
            entity.ApproverName = approverName;
            entity.ApproveRemark = remark;
            entity.ApproveResult = (int) result;
            entity.ApproveAt = DateTime.Now;
            entity.ApproveStep = (int)ApproveStep.Approved;

            return _db.Update(entity);
        }

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

        public int Confirm(int id, ApproveResult result, string remark, string userNo, string userName)
        {
            var entity = _db.Load<Approval>(id);
            if (entity == null)
            {
                return 0;
            }

            entity.ApproveResult = (int) result;
            entity.ConfirmRemark = remark;
            entity.ConfirmNo = userNo;
            entity.ConfirmName = userName;
            entity.ConfirmAt = DateTime.Now;
            entity.ApproveStep = (int) ApproveStep.Confirmed;

            return _db.Update(entity);
        }
    }
}
