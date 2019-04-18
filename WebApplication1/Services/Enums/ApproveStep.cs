using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Services
{
    public enum ApproveStep
    {
        [Display(Name = "已审批")]
        Approved = 1,
        [Display(Name = "财务已审核")]
        Audited = 2,
        [Display(Name = "已确认（二次审批）")]
        Confirmed = 3,
        [Display(Name = "申请已关闭")]
        Close = 4
    }

    public class ApproveStepUtil
    {
        public static string GetDescription(int step)
        {
            switch (step)
            {
                case (int) ApproveStep.Approved:
                    return "第一次审批已完成";
                case (int)ApproveStep.Audited:
                    return "财务已审核";
                case (int)ApproveStep.Confirmed:
                    return "申请已完结";
                case (int)ApproveStep.Close:
                    return "申请已关闭";
                default:
                    return "待审批";
            }
        }
    }
}
