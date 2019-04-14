using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class SupplementDto
    {
        public int Id { get; set; }

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
    }
}
