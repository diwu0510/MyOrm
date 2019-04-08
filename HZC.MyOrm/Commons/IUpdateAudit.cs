using System;

namespace HZC.MyOrm.Commons
{
    public interface IUpdateAudit
    {
        string Updator { get; set; }

        DateTime UpdateAt { get; set; }
    }
}
