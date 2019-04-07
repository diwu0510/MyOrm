using MyOrm.Attributes;
using MyOrm.Commons;
using System;

namespace ConsoleApp1
{
    public class School : IEntity
    {
        [MyKey(FieldName = "SchoolId")]
        public int Id { get; set; }

        public string SchoolType { get; set; }

        public string SchoolName { get; set; }

        [MyColumn(UpdateIgnore = true)]
        public bool IsDel { get; set; }

        [MyColumn(UpdateIgnore = true)]
        public string Owner { get; set; }

        [MyColumn(UpdateIgnore = true)]
        public string CreateBy { get; set; }

        [MyColumn(UpdateIgnore = true)]
        public DateTime CreateAt { get; set; }

        public string UpdateBy { get; set; }

        public DateTime UpdateAt { get; set; }
    }
}
