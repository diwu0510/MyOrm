using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Services
{
    public enum ApproveResult
    {
        [Display(Name = "一次审批通过")]
        ApprovePass = 1,
        [Display(Name = "一次审批被拒绝")]
        ApproveRefuse = 2,
        [Display(Name = "二次审批通过")]
        ConfirmPass = 3,
        [Display(Name = "二次审批被拒绝")]
        ConfirmRefuse = 4
    }

    public class ApproveResultUtil
    {
        public static string GetDescription(int result)
        {
            switch (result)
            {
                case (int) ApproveResult.ApprovePass:
                    return "第一次审批已通过";
                case (int) ApproveResult.ApproveRefuse:
                    return "第一次审批被拒绝";
                case (int) ApproveResult.ConfirmPass:
                    return "第二次审批已通过";
                case (int) ApproveResult.ConfirmRefuse:
                    return "第二次审批被拒绝";
                default:
                    return "未审批";
            }
        }
    }
}
