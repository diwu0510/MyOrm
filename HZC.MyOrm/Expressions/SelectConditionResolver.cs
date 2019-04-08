using HZC.MyOrm.Reflections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace HZC.MyOrm.Expressions
{
    public class SelectConditionResolver<T>
    {
        #region 字段

        private readonly MyEntity _entity;

        private readonly string _prefix;

        private int _parameterIndex;

        private readonly Stack<string> _stack = new Stack<string>();

        private readonly QueryConditionResolveResult _result = new QueryConditionResolveResult();

        #endregion

        #region 构造函数

        public SelectConditionResolver(string prefix = "@")
        {
            _entity = MyEntityContainer.Get(typeof(T));
            _prefix = prefix;
        }

        public SelectConditionResolver(MyEntity entity, string prefix = "@")
        {
            _entity = entity;
            _prefix = prefix;
        }

        #endregion

        #region 返回结果

        public QueryConditionResolveResult GetResult()
        {
            return _result;
        }

        #endregion

        #region 解析表达式

        public void Resolve(Expression node)
        {
            if (node.NodeType == ExpressionType.AndAlso ||
                node.NodeType == ExpressionType.OrElse)
            {
                var expression = (BinaryExpression)node;
                _stack.Push(")");
                _stack.Push(ResolveClause(expression.Right));
                _stack.Push(node.NodeType.ToSqlOperator());
                Resolve(expression.Left);
                _stack.Push("(");
            }
            else
            {
                _stack.Push(ResolveClause(node));
            }

            _result.Condition = string.Concat(_stack);
        }

        #endregion

        #region 解析子句

        private string ResolveClause(Expression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    return ResolveNotClause((UnaryExpression)node);
                case ExpressionType.Constant:
                    return ResolveConstantClause((ConstantExpression)node);
                case ExpressionType.MemberAccess:
                    return ResolveMemberAccessClause((MemberExpression)node);
                case ExpressionType.Call:
                    return ResolveCallClause((MethodCallExpression)node);
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    return ResolveBinaryClause((BinaryExpression)node);

                default:
                    throw new ArgumentException("不受支持的表达式", nameof(node));
            }
        } 

        #endregion

        #region 解析常量表达式子句

        private string ResolveConstantClause(ConstantExpression node)
        {
            var value = (bool)node.Value;
            return value ? "1=1" : "1<>1";
        }

        #endregion

        #region 解析成员访问表达式子句

        private string ResolveMemberAccessClause(MemberExpression node)
        {
            var rootNodeType = node.GetRootType(out var stack);
            if (rootNodeType != ExpressionType.Parameter)
            {
                var value = (bool)node.GetValue();
                return value ? "1=1" : "1<>1";
            }
            else
            {
                var fieldName = ResolveStackToField(stack);
                return $"{fieldName}=1";
            }
        }

        #endregion

        #region 解析Not表达式子句

        private string ResolveNotClause(UnaryExpression node)
        {
            var obj = node.Operand;
            if (obj.NodeType == ExpressionType.MemberAccess)
            {
                var expression = (MemberExpression) obj;
                var rootNodeType = expression.GetRootType(out var stack);

                if (rootNodeType == ExpressionType.Parameter)
                {
                    var fieldName = ResolveStackToField(stack);
                    return $"{fieldName}=1";
                }
            }

            var value = (bool)node.GetValue();
            return value ? "1=1" : "1<>1";
        }

        #endregion

        #region 解析方法调用表达式子句

        private string ResolveCallClause(MethodCallExpression node)
        {
            var obj = node.Object;
            if (obj != null && obj.NodeType == ExpressionType.MemberAccess)
            {
                var expression = (MemberExpression)obj;
                var rootNodeType = expression.GetRootType(out var stack);

                if (rootNodeType == ExpressionType.Parameter)
                {
                    return ResolveParameterMemberMethodCall(node, stack);
                }
            }

            var value = (bool)node.GetValue();
            return value ? "1=1" : "1<>1";
        }

        #endregion

        #region 解析AND|OR子句

        private string ResolveBinaryClause(BinaryExpression node)
        {
            return $"{ResolveExpression(node.Left)} {node.NodeType.ToSqlOperator()} {ResolveExpression(node.Right)}";
        }

        #endregion

        #region 解析表达式

        private string ResolveExpression(Expression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Constant:
                    return ResolveConstant((ConstantExpression)node);
                case ExpressionType.MemberAccess:
                    return ResolveMemberAccess((MemberExpression)node);
                case ExpressionType.Call:
                    return ResolveMethodCall((MethodCallExpression)node);
                default:
                    throw new ArgumentException("不受支持的表达式", nameof(node));
            }
        }

        #endregion

        #region 解析常量表达式

        private string ResolveConstant(ConstantExpression node)
        {
            var parameterName = GetParameterName();

            var value = node.Value;
            _result.Parameters.Add(parameterName, value);

            return parameterName;
        }

        #endregion

        #region 解析成员访问表达式

        private string ResolveMemberAccess(MemberExpression node)
        {
            var rootNodeType = node.GetRootType(out var stack);
            if (rootNodeType == ExpressionType.Parameter)
            {
                var fieldName = ResolveStackToField(stack);
                return fieldName;
            }
            else
            {
                var parameterName = GetParameterName();

                var value = node.GetValue();
                _result.Parameters.Add(parameterName, value);

                return parameterName;
            }
        }

        #endregion

        #region 解析方法表达式

        private string ResolveMethodCall(MethodCallExpression node)
        {
            var obj = node.Object;
            if (obj != null && obj.NodeType == ExpressionType.MemberAccess)
            {
                var expression = (MemberExpression)obj;
                var rootNodeType = expression.GetRootType(out var stack);

                if (rootNodeType == ExpressionType.Parameter)
                {
                    return ResolveParameterMemberMethodCall(node, stack);
                }
            }

            throw new ArgumentException("不支持的表达式");
        }

        #endregion

        #region 解析二元表达式

        private string ResolveBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.AndAlso ||
                node.NodeType == ExpressionType.OrElse)
            {
                var right = ResolveClause(node.Right);
                var op = node.NodeType.ToSqlOperator();
                var left = ResolveClause(node.Left);

                return $"({left} {op} {right})";
            }
            else
            {
                var expression = (BinaryExpression)node;
                var right = expression.Right;
                var left = expression.Left;
                var op = expression.NodeType.ToSqlOperator();

                if (op == "=" || op == "<>")
                {
                    if (right.NodeType == ExpressionType.Constant && right.GetValue() == null)
                    {
                        return op == "="
                            ? $"{ResolveClause(left)} IS NULL"
                            : $"{ResolveClause(left)} IS NOT NULL";
                    }
                }

                return $"{ResolveClause(left)} {op} {ResolveClause(right)}";
            }
        }

        #endregion
        
        #region 解析参数对象方法调用表达式

        private string ResolveParameterMemberMethodCall(MethodCallExpression node, Stack<string> stack)
        {
            var methodName = node.Method.Name;
            if (methodName != "Contains" &&
                methodName != "StartsWith" &&
                methodName != "EndsWith")
            {
                throw new ArgumentException("不受支持的方法", nameof(node));
            }

            var arg = node.Arguments[0].GetValue().ToString();
            switch (methodName)
            {
                case "Contains":
                    arg = $"%{arg}%";
                    break;
                case "StartsWith":
                    arg = $"{arg}%";
                    break;
                case "EndsWith":
                    arg = $"%{arg}";
                    break;
            }

            var pName = GetParameterName();
            _result.Parameters.Add(pName, arg);

            var fieldName = ResolveStackToField(stack);
            return $"{fieldName} LIKE {_prefix}{pName}";
        }
        #endregion

        #region 辅助方法

        private string GetParameterName()
        {
            return $"__p_{_parameterIndex++}";
        }

        private string ResolveStackToField(Stack<string> parameterStack)
        {
            switch (parameterStack.Count)
            {
                case 2:
                {
                    // 调用了导航属性
                    var propertyName = parameterStack.Pop();
                    var propertyFieldName = parameterStack.Pop();

                    var propertyEntity = _result.NavPropertyList.SingleOrDefault(p => p.Name == propertyName);
                    if (propertyEntity == null)
                    {
                        var prop = _entity.Properties.Single(p => p.Name == propertyName);
                        propertyEntity = MyEntityContainer.Get(prop.PropertyInfo.PropertyType);
                        _result.NavPropertyList.Add(propertyEntity);
                    }

                    var propertyProperty = propertyEntity.Properties.Single(p => p.Name == propertyFieldName);
                    return $"[{propertyName}].[{propertyProperty.FieldName}]";
                }
                case 1:
                {
                    var propertyName = parameterStack.Pop();
                    var propInfo = _entity.Properties.Single(p => p.Name == propertyName);
                    return $"[{_entity.TableName}].[{propInfo.FieldName}]";
                }
                default:
                    throw new ArgumentException("尚未支持大于2层属性调用。如 student.Clazz.School.Id>10，请使用类似 student.Clazz.SchoolId > 0 替代");
            }
        }

        #endregion
    }
}
