using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class ApprovalDto
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Display(Name = "申请人编号")]
        public string ApplicantNo { get; set; }

        [Display(Name = "申请人姓名")]
        public string ApplicantName { get; set; }

        [Display(Name = "部门")]
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
    }
}
