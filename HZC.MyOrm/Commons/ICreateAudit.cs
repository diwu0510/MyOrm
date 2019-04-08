using System;

namespace HZC.MyOrm.Commons
{
    public interface ICreateAudit
    {
        DateTime CreateAt { get; set; }

        string Creator { get; set; }
    }
}
