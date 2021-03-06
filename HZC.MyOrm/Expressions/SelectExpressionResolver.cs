﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace HZC.MyOrm.Expressions
{
    public class SelectExpressionResolver
    {
        private readonly List<string> _propertyList;

        private readonly List<SelectResolveResult> _dict;

        private Type _targetType;

        public SelectExpressionResolver()
        {
            _propertyList = new List<string>();
            _dict = new List<SelectResolveResult>();
        }

        public List<SelectResolveResult> GetPropertyList()
        {
            return _dict;
        }

        public Type GetTargetType()
        {
            return _targetType;
        }

        public void Clear()
        {
            _propertyList.Clear();
        }

        public void Visit(LambdaExpression expression)
        {
            if (expression.Body.NodeType == ExpressionType.MemberAccess)
            {
                VisitMember((MemberExpression)expression.Body);
            }
            else if (expression.Body.NodeType == ExpressionType.MemberInit)
            {
                VisitMemberInit((MemberInitExpression)expression.Body);
            }
            else if(expression.Body.NodeType == ExpressionType.New)
            {
                VisitNew((NewExpression)expression.Body);
            }
        }

        /// <summary>
        /// 解析参数成员，如：s => s.Student
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected Expression VisitMember(MemberExpression node)
        {
            var rootType = node.GetRootType(out var stack);
            if (rootType == ExpressionType.Parameter)
            {
                if (stack.Count == 1)
                {
                    var propertyName = stack.Pop();
                    var memberName = node.Member.Name;

                    _dict.Add(new SelectResolveResult
                    {
                        PropertyName = propertyName,
                        MemberName = memberName,
                        FieldName = ""
                    });
                }
                else if (stack.Count == 2)
                {
                    var propertyName = stack.Pop();
                    var fieldName = stack.Pop();
                    var memberName = node.Member.Name;
                    _dict.Add(new SelectResolveResult
                    {
                        MemberName = memberName,
                        PropertyName = propertyName,
                        FieldName = fieldName
                    });
                }
            }
            return node;
        }

        /// <summary>
        /// 解析新建对象表达式 s => new { s.Id }
        /// 暂时用不到，因为还没实现映射匿名类的功能相关功能
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected Expression VisitNew(NewExpression node)
        {
            _targetType = node.Type;
            if (node.Members != null)
            {
                for (var i = 0; i < node.Members.Count; i++)
                {
                    if (node.Arguments[i].NodeType == ExpressionType.MemberAccess)
                    {
                        var member = (MemberExpression) node.Arguments[i];
                        var rootType = member.GetRootType(out var stack);
                        if (rootType == ExpressionType.Parameter)
                        {
                            if (stack.Count == 1)
                            {
                                var propertyName = stack.Pop();
                                var memberName = node.Members[i].Name;

                                _dict.Add(new SelectResolveResult
                                {
                                    PropertyName = propertyName,
                                    MemberName = memberName,
                                    FieldName = ""
                                });
                            }
                            else if (stack.Count == 2)
                            {
                                var propertyName = stack.Pop();
                                var fieldName = stack.Pop();
                                var memberName = node.Members[i].Name;
                                _dict.Add(new SelectResolveResult
                                {
                                    PropertyName = propertyName,
                                    MemberName = memberName,
                                    FieldName = fieldName
                                });
                            }
                        }
                    }
                }
            }

            return node;
        }

        /// <summary>
        /// 解析对象初始化表达式 如：s => new StudentDto { s.Name, s.Id }
        /// </summary>
        /// <param name="node"></param>
        protected void VisitMemberInit(MemberInitExpression node)
        {
            foreach (var binding in node.Bindings)
            {
                if (binding.BindingType == MemberBindingType.Assignment)
                {
                    var expression = ((MemberAssignment) binding).Expression;
                    if (expression.NodeType == ExpressionType.MemberAccess)
                    {
                        var member = (MemberExpression)expression;
                        var rootType = member.GetRootType(out var stack);
                        if (rootType == ExpressionType.Parameter)
                        {
                            if (stack.Count == 1)
                            {
                                var propertyName = stack.Pop();
                                var memberName = binding.Member.Name;

                                _dict.Add(new SelectResolveResult
                                {
                                    PropertyName = propertyName,
                                    MemberName = memberName,
                                    FieldName = ""
                                });
                            }
                            else if (stack.Count == 2)
                            {
                                var propertyName = stack.Pop();
                                var fieldName = stack.Pop();
                                var memberName = binding.Member.Name;
                                _dict.Add(new SelectResolveResult
                                {
                                    PropertyName = propertyName,
                                    MemberName = memberName,
                                    FieldName = fieldName
                                });
                            }
                        }
                    }
                }
            }
        }
    }

    public class SelectResolveResult
    {
        /// <summary>
        /// select对象的成员名称
        /// 如：SchoolName = s.School.Name  中的SchoolName
        /// </summary>
        public string MemberName { get; set; }

        // 右侧的属性名
        /// <summary>
        /// 第一级属性名
        /// 如：SchoolName = s.School.Name 中的School
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// 右侧属性对象的子属性
        /// 如：SchoolName = s.School.Name 中的Name
        /// </summary>
        public string FieldName { get; set; }
    }
}
