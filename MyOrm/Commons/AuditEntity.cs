using System;

namespace MyOrm.Commons
{
    public class AuditEntity : ICreateAudit, IUpdateAudit
    {
        public DateTime CreateAt { get; set; }

        public string Creator { get; set; }

        public string Updator { get; set; }

        public DateTime UpdateAt { get; set; }
    }
}
