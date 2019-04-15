using WebApplication1.Services;

namespace WebApplication1.Models
{
    public class ApproveModel
    {
        public int Id { get; set; }

        public ApproveResult Result { get; set; } 

        public string Remark { get; set; }
    }
}
