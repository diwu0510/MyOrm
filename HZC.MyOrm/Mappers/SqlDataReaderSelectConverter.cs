﻿using HZC.MyOrm.Reflections;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace HZC.MyOrm.Mappers
{
    public class SqlDataReaderSelectConverter
    {
        public Func<SqlDataReader, TTarget> ResolveClass<TTarget>(SqlDataReader sdr)
        {
            if (!sdr.HasRows) return null;

            var sdrParameter = Expression.Parameter(typeof(SqlDataReader), "sdr");
            var memberBindings = new List<MemberBinding>();
            var subMemberMaps = new Dictionary<string, List<IncludePropertySdrMap>>();

            var masterEntity = MyEntityContainer.Get(typeof(TTarget));


            for (var i = 0; i < sdr.FieldCount; i++)
            {
                var fieldName = sdr.GetName(i);
                var fieldNames = fieldName.Split("__");

                if (fieldNames.Length == 1)
                {
                    var property = masterEntity.Properties.Single(p => p.Name == fieldName);
                    if (property != null)
                    {
                        var methodName = GetSdrMethodName(property.PropertyInfo.PropertyType);
                        var methodCall = Expression.Call(sdrParameter,
                            typeof(SqlDataReader).GetMethod(methodName) ?? throw new InvalidOperationException(),
                            Expression.Constant(i));

                        Expression setValueExpression;
                        if (property.PropertyInfo.PropertyType.IsGenericType &&
                            property.PropertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            setValueExpression = Expression.Convert(methodCall, property.PropertyInfo.PropertyType);
                        }
                        else
                        {
                            setValueExpression = methodCall;
                        }

                        memberBindings.Add(
                            Expression.Bind(
                                property.PropertyInfo,
                                Expression.Condition(
                                    Expression.TypeIs(
                                        Expression.Call(
                                            sdrParameter,
                                            typeof(SqlDataReader).GetMethod("get_Item", new[] { typeof(int) }) ??
                                            throw new InvalidOperationException(),
                                            Expression.Constant(i)),
                                        typeof(DBNull)
                                    ),
                                    Expression.Default(property.PropertyInfo.PropertyType),
                                    setValueExpression
                                )
                            )
                        );
                    }
                }
                else
                {
                    if (subMemberMaps.TryGetValue(fieldNames[0], out var list))
                    {
                        list.Add(new IncludePropertySdrMap { PropertyName = fieldNames[1], Index = i });
                    }
                }
            }

            foreach (var include in subMemberMaps)
            {
                var prop = masterEntity.Properties.Single(p => p.Name == include.Key);
                if (prop != null)
                {
                    var subEntityInfo = MyEntityContainer.Get(prop.PropertyInfo.PropertyType);
                    var subBindingList = new List<MemberBinding>();
                    foreach (var subProperty in subEntityInfo.Properties)
                    {
                        if (subProperty.IsMap)
                        {
                            var mapper = include.Value.SingleOrDefault(v => v.PropertyName == subProperty.Name);
                            if (mapper != null)
                            {
                                var methodName = GetSdrMethodName(subProperty.PropertyInfo.PropertyType);
                                var methodCall = Expression.Call(
                                    sdrParameter,
                                    typeof(SqlDataReader).GetMethod(methodName) ??
                                    throw new InvalidOperationException(),
                                    Expression.Constant(mapper.Index));

                                Expression setValueExpression;
                                if (subProperty.PropertyInfo.PropertyType.IsGenericType &&
                                    subProperty.PropertyInfo.PropertyType.GetGenericTypeDefinition() ==
                                    typeof(Nullable<>))
                                {
                                    setValueExpression = Expression.Convert(methodCall,
                                        subProperty.PropertyInfo.PropertyType);
                                }
                                else
                                {
                                    setValueExpression = methodCall;
                                }

                                subBindingList.Add(
                                    Expression.Bind(
                                        subProperty.PropertyInfo,
                                        Expression.Condition(
                                            Expression.TypeIs(
                                                Expression.Call(
                                                    sdrParameter,
                                                    typeof(SqlDataReader).GetMethod("get_Item",
                                                        new[] { typeof(int) }) ??
                                                    throw new InvalidOperationException(),
                                                    Expression.Constant(mapper.Index)),
                                                typeof(DBNull)
                                            ),
                                            Expression.Default(subProperty.PropertyInfo.PropertyType),
                                            setValueExpression
                                        )
                                    )
                                );
                            }
                        }

                        var subInitExpression = Expression.MemberInit(
                            Expression.New(prop.PropertyInfo.PropertyType),
                            subBindingList);
                        memberBindings.Add(Expression.Bind(prop.PropertyInfo, subInitExpression));
                    }
                }
            }

            var initExpression = Expression.MemberInit(Expression.New(typeof(TTarget)), memberBindings);
            return Expression.Lambda<Func<SqlDataReader, TTarget>>(initExpression, sdrParameter).Compile();
        }

        public Func<SqlDataReader, TTarget> ResolveConstant<TTarget>(SqlDataReader sdr, string fieldName = "")
        {
            var type = typeof(TTarget);
            var sdrParameter = Expression.Parameter(typeof(SqlDataReader), "sdr");
            MethodCallExpression callExpression;
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                var methodName = GetSdrMethodName(type);
                callExpression = Expression.Call(sdrParameter, typeof(SqlDataReader).GetMethod(methodName),
                    Expression.Constant(0));
                return Expression.Lambda<Func<SqlDataReader, TTarget>>(callExpression, sdrParameter).Compile();
            }
            else
            {
                callExpression = Expression.Call(sdrParameter,
                    typeof(SqlDataReader).GetMethod("get_item", new[] {typeof(string)}),
                    Expression.Constant(fieldName));
                var convertExpression = Expression.Convert(callExpression, type);
                return Expression.Lambda<Func<SqlDataReader, TTarget>>(convertExpression, sdrParameter).Compile();
            }
        }

        public Func<SqlDataReader, Dictionary<string, object>> ResolveObject(SqlDataReader sdr)
        {
            var sdrParameter = Expression.Parameter(typeof(SqlDataReader), "sdr");

            var newExpression = Expression.Variable(typeof(Dictionary<string, object>), "dict");
            
            var count = sdr.FieldCount;
            var iExpression = Expression.Variable(typeof(int), "i");
            var initIExpression = Expression.Assign(iExpression, Expression.Constant(0));

            var label = Expression.Label(typeof(Dictionary<string, object>));

            var block2 = Expression.Block(
                            Expression.Call(
                                newExpression,
                                typeof(IDictionary<string, object>).GetMethod("Add"),
                                Expression.Call(sdrParameter, typeof(SqlDataReader).GetMethod("GetName", new[] { typeof(int) }), iExpression),
                                Expression.Call(sdrParameter, typeof(SqlDataReader).GetMethod("GetValue", new[] { typeof(int) }), iExpression)),
                            Expression.PostIncrementAssign(iExpression)
                        );

            var loop = Expression.Loop(
                 Expression.IfThenElse(
                        Expression.LessThan(iExpression, Expression.Constant(count)),
                        block2,
                        Expression.Break(label, newExpression)
                    ),
                    label
                );

            var block = Expression.Block(
                new[] { newExpression, iExpression },
                initIExpression,
                Expression.Assign(newExpression, Expression.New(typeof(Dictionary<string, object>))),
                loop);

            var lambda = Expression.Lambda<Func<SqlDataReader, Dictionary<string, object>>>(block, sdrParameter);
            return lambda.Compile();
        }

        public List<T> ConvertToList<T>(SqlDataReader sdr)
        {
            var result = new List<T>();
            if (!sdr.HasRows)
            {
                return result;
            }

            Func<SqlDataReader, T> func;
            if (typeof(T).IsClass && typeof(T) != typeof(string))
            {
                func = ResolveClass<T>(sdr);
            }
            else
            {
                func = ResolveConstant<T>(sdr);
            }

            do
            {
                while (sdr.Read())
                {
                    result.Add(func(sdr));
                }
            } while (sdr.NextResult());

            return result;
        }

        public List<dynamic> ConvertToDynamicList(SqlDataReader sdr)
        {
            var result = new List<dynamic>();
            var func = ResolveObject(sdr);
            do
            {
                while (sdr.Read())
                {
                    var r = func(sdr);
                    dynamic rr = new System.Dynamic.ExpandoObject();
                    foreach(var kv in r)
                    {
                        ((IDictionary<string, object>)rr).Add(kv.Key, kv.Value);
                    }
                    result.Add(rr);
                }
            } while (sdr.NextResult());
            return result;
        }

        public T ConvertToEntity<T>(SqlDataReader sdr)
        {
            if (!sdr.HasRows)
            {
                return default(T);
            }

            var func = typeof(T).IsClass ? ResolveClass<T>(sdr) : ResolveConstant<T>(sdr);

            if (func == null)
            {
                return default(T);
            }

            return sdr.Read() ? func.Invoke(sdr) : default(T);
        }

        public dynamic ConvertToDynamicEntity(SqlDataReader sdr)
        {
            var func = ResolveObject(sdr);
            do
            {
                if (sdr.Read())
                {
                    var r = func(sdr);
                    dynamic rr = new System.Dynamic.ExpandoObject();
                    foreach (var kv in r)
                    {
                        ((IDictionary<string, object>)rr).Add(kv.Key, kv.Value);
                    }

                    return rr;
                }
            } while (sdr.NextResult());
            return null;
        }

        private static string GetSdrMethodName(Type type)
        {
            var realType = GetRealType(type);
            string methodName;

            if (realType == typeof(string))
            {
                methodName = "GetString";
            }
            else if (realType == typeof(int))
            {
                methodName = "GetInt32";
            }
            else if (realType == typeof(DateTime))
            {
                methodName = "GetDateTime";
            }
            else if (realType == typeof(decimal))
            {
                methodName = "GetDecimal";
            }
            else if (realType == typeof(Guid))
            {
                methodName = "GetGuid";
            }
            else if (realType == typeof(bool))
            {
                methodName = "GetBoolean";
            }
            else
            {
                throw new ArgumentException($"不受支持的类型:{type.FullName}");
            }

            return methodName;
        }

        private static Type GetRealType(Type type)
        {
            var realType = type.IsGenericType &&
                           type.GetGenericTypeDefinition() == typeof(Nullable<>)
                ? type.GetGenericArguments()[0]
                : type;

            return realType;
        }
    }
}
