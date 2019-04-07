using System;

namespace MyOrm.Commons
{
    public interface ICreateAudit
    {
        DateTime CreateAt { get; set; }

        string Creator { get; set; }
    }
}
