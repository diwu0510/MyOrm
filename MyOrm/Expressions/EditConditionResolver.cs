using MyOrm.Reflections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyOrm.Expressions
{
    public class EditConditionResolver<T> : BaseConditionResolver<T>
    {
        public EditConditionResolver(string prefix = "@") : base(prefix)
        { }

        public EditConditionResolver(MyEntity entity, string prefix = "@") : base(entity, prefix)
        { }

        protected override string ResolveStackToField(Stack<string> parameterStack)
        {
            if (parameterStack.Count != 1)
                throw new ArgumentException(
                    "不支持大于1层属性调用");

            var propertyName = parameterStack.Pop();
            var propInfo = Entity.Properties.Single(p => p.Name == propertyName);
            return $"[{Entity.TableName}].[{propInfo.FieldName}]";
        }
    }
}
