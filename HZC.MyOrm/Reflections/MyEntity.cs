using HZC.MyOrm.Attributes;
using HZC.MyOrm.Commons;
using System;
using System.Collections.Generic;

namespace HZC.MyOrm.Reflections
{
    public class MyEntity
    {
        public string KeyColumn { get; set; }

        public string Name { get; set; }

        public string TableName { get; set; }

        public bool IsSoftDelete { get; set; }

        public bool IsCreateAudit { get; set; }

        public bool IsUpdateAudit { get; set; }

        public List<MyProperty> Properties { get; set; }

        public MyEntity(Type type)
        {
            Name = type.Name;
            IsSoftDelete = type.IsInstanceOfType(typeof(ISoftDelete));
            IsCreateAudit = type.IsInstanceOfType(typeof(ICreateAudit));
            IsUpdateAudit = type.IsInstanceOfType(typeof(IUpdateAudit));

            var tableAttr = type.GetCustomAttributes(typeof(MyTableAttribute), false);
            if (tableAttr.Length > 0)
            {
                var tableName = ((MyTableAttribute)tableAttr[0]).TableName;
                TableName = string.IsNullOrWhiteSpace(tableName) ? type.Name.Replace("Entity", "") : tableName;
            }
            else
            {
                TableName = Name;
            }

            Properties = new List<MyProperty>();

            foreach (var propertyInfo in type.GetProperties())
            {
                var property = new MyProperty(propertyInfo);
                if (property.IsKey)
                {
                    KeyColumn = property.FieldName;
                }
                Properties.Add(property);
                MyEntityMapperContainer.Add($"{Name}-{property.Name}", property.FieldName);
            }
        }

        public string GetFiledName(string propertyName)
        {
            return MyEntityMapperContainer.Get(propertyName);
        }
    }
}
