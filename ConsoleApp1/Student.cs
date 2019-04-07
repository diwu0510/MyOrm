using MyOrm.Attributes;
using MyOrm.Commons;
using System;

namespace ConsoleApp1
{
    public class Student : IEntity
    {
        // 数据表的主键名称为StudentId
        [MyKey(FieldName = "StudentId")]
        public int Id { get; set; }

        public string StudentName { get; set; }

        public string Mobile { get; set; }

        public string Card { get; set; }

        public string State { get; set; }

        public DateTime? Birthday { get; set; }

        // 更新时忽略学校Id
        [MyColumn(UpdateIgnore = true)]
        public int FKSchoolId { get; set; }

        [MyColumn(UpdateIgnore = true)]
        public string Owner { get; set; }

        // 是否删除字段只能通过 Delete 方法修改，Update时需忽略该属性
        [MyColumn(UpdateIgnore = true)]
        public bool IsDel { get; set; }

        // 创建时间和创建人只在创建时指定，其他时候不能修改
        [MyColumn(UpdateIgnore = true)]
        public DateTime CreateAt { get; set; }

        [MyColumn(UpdateIgnore = true)]
        public string CreateBy { get; set; }

        public DateTime UpdateAt { get; set; }

        public string UpdateBy { get; set; }

        // 导航属性的外键为 FKSchoolId，若不指定，默认为SchoolId
        [MyForeignKey("FKSchoolId")]
        public School School { get; set; }
    }
}
