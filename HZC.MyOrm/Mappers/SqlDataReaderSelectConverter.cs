using HZC.MyOrm.Reflections;
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

        public Func<SqlDataReader, dynamic> Resolve2(SqlDataReader sdr)
        {
            var sdrParameter = Expression.Parameter(typeof(SqlDataReader), "sdr");
            var newExpression = Expression.New(typeof(System.Dynamic.ExpandoObject));
            var convertExpression = Expression.Convert(newExpression, typeof(IDictionary<string, object>));

            var memberBindings = new List<MemberBinding>();
            for(var i = 0; i < sdr.FieldCount; i++)
            {
                var nameExpression = Expression.Call(sdrParameter, typeof(SqlDataReader).GetMethod("GetName"), Expression.Constant(i));
                //var itemExpression = Expression.Call(
                //            sdrParameter,
                //            typeof(SqlDataReader).GetMethod("get_Item",
                //                new[] { typeof(int) }) ??
                //            throw new InvalidOperationException(),
                //            Expression.Constant(i));
                //var type = Expression.Constant(Expression.Call(sdrParameter, typeof(SqlDataReader).GetMethod("GetFieldType", new[] { typeof(int) }), Expression.Constant(i)));
                var valueExpression = Expression.Call(sdrParameter, typeof(SqlDataReader).GetMethod("GetValue", new[] { typeof(int) }), Expression.Constant(i));

                //var callExpression = Expression.Call(
                //    convertExpression,
                //    typeof(IDictionary<string, object>).GetMethod("Add"),
                //    nameExpression, valueExpression);

                Expression.Call(newExpression,
                    typeof(System.Dynamic.ExpandoObject).GetMethod("TryAdd", new[] { typeof(string), typeof(object) }),
                    nameExpression,
                    valueExpression);
            }
            var initExpression = Expression.MemberInit(newExpression);
            var lambda = Expression.Lambda<Func<SqlDataReader, dynamic>>(initExpression, sdrParameter);
            return lambda.Compile();
        }

        public List<T> ConvertToList<T>(SqlDataReader sdr)
        {
            var result = new List<T>();
            if (!sdr.HasRows)
            {
                return result;
            }

            var func = typeof(T).IsClass && typeof(T) != typeof(string) ? ResolveClass<T>(sdr) : ResolveConstant<T>(sdr);

            if (func == null)
            {
                return result;
            }

            while (sdr.Read())
            {
                result.Add(func.Invoke(sdr));
            }

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

            if (sdr.Read())
            {
                return func.Invoke(sdr);
            }

            return default(T);
        }

        public List<dynamic> ConvertToList(SqlDataReader sdr)
        {
            var result = new List<dynamic>();
            if (!sdr.HasRows)
            {
                return result;
            }

            var func = Resolve2(sdr);

            if (func == null)
            {
                return result;
            }

            while (sdr.Read())
            {
                result.Add(func.Invoke(sdr));
            }

            return result;
        }

        public dynamic ConvertToEntity(SqlDataReader sdr)
        {
            if (!sdr.HasRows)
            {
                return null;
            }

            var func = Resolve2(sdr);

            if (func != null && sdr.Read())
            {
                return func.Invoke(sdr);
            }

            return null;
        }

        private string GetSdrMethodName(Type type)
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

        public Type ConvertSdrFieldToType(SqlDataReader sdr)
        {
            return null;
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
