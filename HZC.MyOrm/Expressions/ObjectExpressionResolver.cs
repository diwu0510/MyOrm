using HZC.MyOrm.Commons;
using System;
using System.Linq.Expressions;

namespace HZC.MyOrm.Expressions
{
    public class ObjectExpressionResolver
    {
        public static DbKvs Resolve(Expression expr)
        {
            if (expr.NodeType != ExpressionType.New)
            {
                throw new ArgumentException("不受支持的表达式，当前仅支持 t => new {property = value}的形式");
            }
            return ResolveNew((NewExpression)expr);
        }

        private static DbKvs ResolveNew(NewExpression node)
        {
            var kvs = DbKvs.New();
            var members = node.Members;
            var args = node.Arguments;

            for (var i = 0; i < members.Count; i++)
            {
                var key = members[i].Name;
                var val = args[i].GetValue();

                kvs.Add(key, val);
            }

            return kvs;
        }
    }
}
