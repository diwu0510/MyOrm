using HZC.MyOrm.Reflections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace HZC.MyOrm.Expressions
{
    public abstract class BaseConditionResolver<T>
    {
        #region 字段

        protected MyEntity Entity { get; }

        private readonly string _prefix;

        private readonly Stack<string> _stack = new Stack<string>();

        protected readonly QueryConditionResolveResult Result = new QueryConditionResolveResult();

        private int _parameterIndex;

        #endregion

        #region 构造函数

        protected BaseConditionResolver(string prefix = "@")
        {
            Entity = MyEntityContainer.Get(typeof(T));
            _prefix = prefix;
        }

        protected BaseConditionResolver(MyEntity entity, string prefix = "@")
        {
            Entity = entity;
            _prefix = prefix;
        }

        #endregion

        #region 返回结果

        public QueryConditionResolveResult Resolve(Expression expression)
        {
            Visit(expression);
            var condition = string.Concat(_stack.ToArray());
            Result.Condition = condition;
            _stack.Clear();
            return Result;
        }
        #endregion

        #region 处理表达式目录树

        private void Visit(Expression node)
        {
            if (node.NodeType == ExpressionType.AndAlso ||
                node.NodeType == ExpressionType.OrElse)
            {
                var expression = (BinaryExpression)node;
                var right = expression.Right;
                var left = expression.Left;

                var rightString = ResolveExpression(right);
                var op = node.NodeType.ToSqlOperator();

                _stack.Push(")");
                _stack.Push(rightString);
                _stack.Push(op);
                Visit(left);
                _stack.Push("(");
            }
            else
            {
                _stack.Push(ResolveExpression(node));
            }
        }

        #endregion

        #region 解析表达式

        private string ResolveExpression(Expression node, bool isClause = true)
        {
            switch (node.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    {
                        var expression = (BinaryExpression)node;
                        var right = ResolveExpression(expression.Right, false);
                        var op = node.NodeType.ToSqlOperator();
                        var left = ResolveExpression(expression.Left, false);

                        return $"({left} {op} {right})";
                    }
                case ExpressionType.MemberAccess:
                    {
                        var expression = (MemberExpression)node;
                        var rootType = expression.GetRootType(out var stack);

                        // 如果是参数表达式
                        if (rootType == ExpressionType.Parameter)
                        {
                            // 如果独立的语句，如 s.IsActive ，则返回 [列名]=1；
                            return isClause ? $"{ResolveStackToField(stack)}=1" : $"{ResolveStackToField(stack)}";
                        }

                        // 如果不是参数表达式，则计算表达式的值（可能是本地变量、常数等）
                        var val = node.GetValue();
                        if (isClause)   // var isActive=true; s => isActive
                        {
                            if (val is bool b)
                            {
                                return b ? "1=1" : "1=0";
                            }
                        } 
                        var parameterName = GetParameterName();
                        Result.Parameters.Add(parameterName, val);
                        return parameterName;
                    }
                case ExpressionType.Call:
                    {
                        // 方法调用
                        var expression = (MethodCallExpression)node;
                        var method = expression.Method.Name;

                        if (expression.Object != null &&
                            expression.Object.NodeType == ExpressionType.MemberAccess)
                        {
                            var rootType = ((MemberExpression)expression.Object).GetRootType(out var stack);
                            if (rootType == ExpressionType.Parameter)
                            {
                                var value = expression.Arguments[0].GetValue();
                                switch (method)
                                {
                                    case "Contains":
                                        value = $"%{value}%";
                                        break;
                                    case "StartsWith":
                                        value = $"{value}%";
                                        break;
                                    case "EndsWith":
                                        value = $"%{value}";
                                        break;
                                }

                                var parameterName = GetParameterName();
                                Result.Parameters.Add(parameterName, value);
                                return $"{ResolveStackToField(stack)} LIKE {parameterName}";
                            }
                            else
                            {
                                var value = node.GetValue();
                                if (isClause)
                                {
                                    if (value is bool b)
                                    {
                                        return b ? "1=1" : "1=0";
                                    }
                                }
                                var parameterName = GetParameterName();
                                Result.Parameters.Add(parameterName, value);
                                return $"{parameterName}";
                            }
                        }
                        else
                        {
                            var value = node.GetValue();
                            if (isClause)
                            {
                                if (value is bool b)
                                {
                                    return b ? "1=1" : "1=0";
                                }
                            }
                            var parameterName = GetParameterName();
                            Result.Parameters.Add(parameterName, value);
                            return $"{parameterName}";
                        }
                    }
                case ExpressionType.Not:
                    {
                        var expression = ((UnaryExpression)node).Operand;
                        if (expression.NodeType == ExpressionType.MemberAccess)
                        {
                            var rootType = ((MemberExpression)expression).GetRootType(out var stack);
                            if (rootType == ExpressionType.Parameter)
                            {
                                return $"{ResolveStackToField(stack)}=0";
                            }
                        }

                        break;
                    }
                // 常量、本地变量
                case ExpressionType.Constant when !isClause:
                    {
                        var val = node.GetValue();
                        var parameterName = GetParameterName();
                        Result.Parameters.Add(parameterName, val);
                        return parameterName;
                    }
                case ExpressionType.Constant:
                    {
                        var expression = (ConstantExpression)node;
                        var value = expression.Value;
                        return value is bool b ? b ? "1=1" : "1=0" : string.Empty;
                    }
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    {
                        // 二元操作符，等于、不等于、大于、小于等
                        var expression = (BinaryExpression)node;
                        var right = expression.Right;
                        var left = expression.Left;
                        var op = expression.NodeType.ToSqlOperator();

                        if (op == "=" || op == "<>")
                        {
                            if (right.NodeType == ExpressionType.Constant && right.GetValue() == null)
                            {
                                return op == "="
                                    ? $"{ResolveExpression(left, false)} IS NULL"
                                    : $"{ResolveExpression(left, false)} IS NOT NULL";
                            }
                        }

                        return $"{ResolveExpression(left, false)} {op} {ResolveExpression(right, false)}";
                    }
                default:
                    {
                        var value = node.GetValue();
                        if (isClause)
                        {
                            return value is bool b ? b ? "1=1" : "1=0" : string.Empty;
                        }

                        var parameterName = GetParameterName();
                        Result.Parameters.Add(parameterName, value);
                        return parameterName;
                    }
            }

            return string.Empty;
        }

        #endregion

        #region 辅助方法

        protected abstract string ResolveStackToField(Stack<string> parameterStack);

        private string GetParameterName()
        {
            return $"{_prefix}__p_{_parameterIndex++}";
        }

        #endregion
    }
}
