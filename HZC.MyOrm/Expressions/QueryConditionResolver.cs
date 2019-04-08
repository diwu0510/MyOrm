using HZC.MyOrm.Reflections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HZC.MyOrm.Expressions
{
    public class QueryConditionResolver<T> : BaseConditionResolver<T>
    {
        public QueryConditionResolver(string prefix = "@") : base(prefix)
        { }

        public QueryConditionResolver(MyEntity entity, string prefix = "@") : base(entity, prefix)
        { }

        protected override string ResolveStackToField(Stack<string> parameterStack)
        {
            switch (parameterStack.Count)
            {
                case 2:
                {
                    // 调用了导航属性
                    var propertyName = parameterStack.Pop();
                    var propertyFieldName = parameterStack.Pop();

                    MyEntity propertyEntity = Result.NavPropertyList.SingleOrDefault(p => p.Name == propertyName);
                    if(propertyEntity == null)
                    {
                        var prop = Entity.Properties.Single(p => p.Name == propertyName);
                        propertyEntity = MyEntityContainer.Get(prop.PropertyInfo.PropertyType);
                        Result.NavPropertyList.Add(propertyEntity);
                    }
                    
                    var propertyProperty = propertyEntity.Properties.Single(p => p.Name == propertyFieldName);
                    return $"[{propertyName}].[{propertyProperty.FieldName}]";
                }
                case 1:
                {
                    var propertyName = parameterStack.Pop();
                    var propInfo = Entity.Properties.Single(p => p.Name == propertyName);
                    return $"[{Entity.TableName}].[{propInfo.FieldName}]";
                }
                default:
                    throw new ArgumentException("尚未支持大于2层属性调用。如 student.Clazz.School.Id>10，请使用类似 student.Clazz.SchoolId > 0 替代");
            }
        }
    }
}
