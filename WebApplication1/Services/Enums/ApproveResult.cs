using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Services
{
    public enum ApproveResult
    {
        [Display(Name = "一次审批通过")]
        ApprovePass,
        [Display(Name = "一次审批被拒绝")]
        ApproveRefuse,
        [Display(Name = "二次审批通过")]
        ConfirmPass,
        [Display(Name = "二次审批被拒绝")]
        ConfirmRefuse
    }
}
