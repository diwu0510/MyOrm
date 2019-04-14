using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Services
{
    public enum ApproveStep
    {
        [Display(Name = "已审批")]
        Approved,
        [Display(Name = "财务已审核")]
        Audited,
        [Display(Name = "已确认（二次审批）")]
        Confirmed,
        [Display(Name = "申请已关闭")]
        Close
    }
}
