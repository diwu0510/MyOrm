using System;

namespace MyOrm.Commons
{
    public interface IUpdateAudit
    {
        string Updator { get; set; }

        DateTime UpdateAt { get; set; }
    }
}
