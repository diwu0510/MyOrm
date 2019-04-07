using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MyOrm.Expressions
{
    public static class ExpressionExtensions
    {
        public static string ToSqlOperator(this ExpressionType type)
        {
            switch (type)
            {
                case (ExpressionType.AndAlso):
                case (ExpressionType.And):
                    return " AND ";
                case (ExpressionType.OrElse):
                case (ExpressionType.Or):
                    return " OR ";
                case (ExpressionType.Not):
                    return " NOT ";
                case (ExpressionType.NotEqual):
                    return "<>";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case (ExpressionType.Equal):
                    return "=";
                default:
                    throw new Exception("不支持该方法");
            }
        }

        public static ExpressionType GetRootType(this MemberExpression expression, out Stack<string> stack)
        {
            var memberExpr = expression;
            var parentExpr = expression.Expression;

            stack = new Stack<string>();
            stack.Push(expression.Member.Name);

            while (parentExpr != null && parentExpr.NodeType == ExpressionType.MemberAccess)
            {
                memberExpr = (MemberExpression)parentExpr;
                parentExpr = ((MemberExpression)parentExpr).Expression;
                stack.Push(memberExpr.Member.Name);
            }

            return parentExpr?.NodeType ?? memberExpr.NodeType;
        }

        public static object GetValue(this Expression expression)
        {
            if (expression.NodeType == ExpressionType.Constant)
            {
                return ((ConstantExpression)expression).Value;
            }
            else
            {
                var cast = Expression.Convert(expression, typeof(object));
                return Expression.Lambda<Func<object>>(cast).Compile().Invoke();
            }
        }
    }
}
