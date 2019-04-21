using System;

namespace WebApplication1.Models
{
    public class ApprovalSearchParameter
    {
        public int Department { get; set; }

        public int Result { get; set; }

        public int Step { get; set; }

        public string Creator { get; set; }

        public string Approver { get; set; }

        public DateTime? CreateAtStart { get; set; }

        public DateTime? CreateAtEnd { get; set; }
    }
}
