using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class AuditModel
    {
        public int Id { get; set; }

        [Display(Name = "核算利润")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal AuditProfit { get; set; }

        [Display(Name = "实际支付金额")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal ActualServiceAmount { get; set; }
    }
}
