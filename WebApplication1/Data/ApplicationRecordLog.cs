using System;
using HZC.MyOrm.Attributes;
using HZC.MyOrm.Commons;

namespace WebApplication1
{
    [MyTable("ApplicationRecordLogs")]
    public class ApplicationRecordLog : IEntity
    {
        public int Id { get; set; }

        public int ApplicationRecordId { get; set; }

        public int OperateType { get; set; }

        public string Details { get; set; }

        [MyColumn(UpdateIgnore = true)]
        public DateTime CreateAt { get; set; }

        public ApplicationRecord ApplicationRecord { get; set; }
    }
}
