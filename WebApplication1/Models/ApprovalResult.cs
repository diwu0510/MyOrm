using System.Collections.Generic;
using WebApplication1.Data;

namespace WebApplication1.Models
{
    public class ApprovalResult : ApprovalSearchParameter
    {
        public List<Approval> Data { get; set; }

        public int PageIndex { get; set; }

        public int PageCount { get; set; }

        public int RecordCount { get; set; }
    }
}
