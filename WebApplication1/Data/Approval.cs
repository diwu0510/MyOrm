using HZC.MyOrm.Attributes;
using HZC.MyOrm.Commons;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Data
{
    [MyTable("Approvals")]
    public class Approval : IEntity
    {
        public Approval()
        {
            CreateAt = DateTime.Now;
        }

        [Display(Name = "ID")]
        public int Id { get; set; }

        [Display(Name = "申请人编号")]
        [MyColumn(UpdateIgnore = true)]
        public string ApplicantNo { get; set; }

        [Display(Name = "申请人姓名")]
        [MyColumn(UpdateIgnore = true)]
        public string ApplicantName { get; set; }

        [Display(Name = "部门")]
        [MyColumn(UpdateIgnore = true)]
        public int DepartmentId { get; set; }

        [Display(Name = "客户单位")]
        [Required(ErrorMessage = "客户单位不能为空")]
        public string CustomerUnit { get; set; }

        [Display(Name = "客户姓名")]
        [Required(ErrorMessage = "客户姓名不能为空")]
        public string CustomerName { get; set; }

        [Display(Name = "客户职位")]
        [Required(ErrorMessage = "客户职位不能为空")]
        public string CustomerJob { get; set; }

        [Display(Name = "联系电话")]
        [Required(ErrorMessage = "联系电话不能为空")]
        public string ContactNumber { get; set; }

        [Display(Name = "项目名称/内容")]
        public string ProjectName { get; set; }

        [Display(Name = "预计成交时间")]
        [Required(ErrorMessage = "预计成交时间不能为空")]
        [DataType(DataType.Date, ErrorMessage = "必须是日期类型")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ExpectedClosingDate { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "预计成交金额")]
        [Required(ErrorMessage = "预计成交金额不能为空")]
        public decimal ExpectedClosingCost { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "预计利润")]
        [Required(ErrorMessage = "预计利润不能为空")]
        public decimal ExpectedClosingProfit { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "申请金额")]
        [Required(ErrorMessage = "申请金额不能为空")]
        public decimal AppliedAmount { get; set; }

        [Display(Name = "申请理由")]
        [Required(ErrorMessage = "申请理由不能为空")]
        public string AppliedReason { get; set; }

        [Display(Name = "审批说明")]
        public string ApproveRemark { get; set; }

        [Display(Name = "审批人编号")]
        public string ApproverNo { get; set; }

        [Display(Name = "审批人姓名")]
        public string ApproverName { get; set; }

        [Display(Name = "审批时间")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime? ApproveAt { get; set; }

        [Display(Name = "订单编号")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string OrderNo { get; set; }

        [Display(Name = "是否有发票")]
        public bool IsHasInvoice { get; set; }

        [Display(Name = "税点")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TaxPoint { get; set; }

        [Display(Name = "付款信息")]
        public string PaymentInfo { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "实际成交金额")]
        public decimal ActualClosingAmount { get; set; }

        [Display(Name = "实际成交利润")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal ActualClosingProfit { get; set; }

        [Display(Name = "回款信息")]
        public string Collections { get; set; }

        [Display(Name = "完成时间")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime? CompleteAt { get; set; }

        [Display(Name = "核算利润")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal AuditProfit { get; set; }

        [Display(Name = "实际支付金额")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal ActualServiceAmount { get; set; }

        [Display(Name = "财务")]
        public string AuditorNo { get; set; }

        [Display(Name = "财务审核人")]
        public string AuditorName { get; set; }

        [Display(Name = "财务审核时间")]
        public DateTime? AuditAt { get; set; }

        [Display(Name = "确认人编号")]
        public string ConfirmNo { get; set; }

        [Display(Name = "确认人姓名")]
        public string ConfirmName { get; set; }

        [Display(Name = "确认说明")]
        public string ConfirmRemark { get; set; }

        [Display(Name = "确认时间")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? ConfirmAt { get; set; }

        [Display(Name = "申请时间")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        [MyColumn(UpdateIgnore = true)]
        public DateTime CreateAt { get; set; }

        [Display(Name = "审批结果")]
        public int ApproveResult { get; set; }

        [Display(Name = "申请阶段")]
        public int ApproveStep { get; set; }

        public Department Department { get; set; }
    }
}
