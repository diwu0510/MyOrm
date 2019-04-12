using HZC.MyOrm.Commons;
using HZC.MyOrm.DbParameters;
using HZC.MyOrm.Expressions;
using HZC.MyOrm.Mappers;
using HZC.MyOrm.Reflections;
using HZC.MyOrm.SqlBuilder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HZC.MyOrm.Queryable
{
    public class MyQueryable<T> where T : class , new ()
    {
        private readonly string _connectionString;

        // 要查询的导航属性
        private readonly List<IncludeProperty> _includeProperties = new List<IncludeProperty>();

        // Where子句中包含导航属性
        private List<string> _whereProperties = new List<string>();

        // 导航属性的缓存
        private readonly List<MyEntity> _entityCache = new List<MyEntity>();

        // Select子句
        private readonly List<SelectResolveResult> _selectProperties = new List<SelectResolveResult>();

        // 主表信息
        private readonly MyEntity _masterEntity;

        // 查询需要的参数
        private readonly MyDbParameters _parameters = new MyDbParameters();

        // 是否已经调用过Where方法
        private bool _hasInitWhere;

        // 拼接好的where子句
        private string _where;

        // 拼接好的order by子句
        private string _orderBy;

        // 构造方法
        public MyQueryable(string connectionString)
        {
            _masterEntity = MyEntityContainer.Get(typeof(T));
            _connectionString = connectionString;
        }

        #region Include
        public MyQueryable<T> Include<TProperty>(Expression<Func<T, TProperty>> expression) where TProperty : IEntity
        {
            if (expression.Body.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpr = (MemberExpression)expression.Body;
                if (memberExpr.Expression != null &&
                    memberExpr.Expression.NodeType == ExpressionType.Parameter &&
                    memberExpr.Member.GetType().IsClass)
                {
                    var property = _includeProperties.SingleOrDefault(p => p.PropertyName == memberExpr.Member.Name);
                    if (property != null)
                    {
                        _includeProperties.Remove(property);
                    }
                    _includeProperties.Add(new IncludeProperty(memberExpr.Member.Name, null));
                }
            }

            return this;
        }

        public MyQueryable<T> Include<TProperty>(
            Expression<Func<T, TProperty>> property, 
            Expression<Func<TProperty, object>> fields) where TProperty : IEntity
        {
            if (property.Body.NodeType == ExpressionType.MemberAccess)
            {
                var visitor = new ObjectMemberVisitor();
                visitor.Visit(property);
                var member = visitor.GetPropertyList().First();

                visitor.Clear();
                visitor.Visit(fields);
                var fieldList = visitor.GetPropertyList();

                var prop = _includeProperties.SingleOrDefault(p => p.PropertyName == member);
                if (prop != null)
                {
                    _includeProperties.Remove(prop);
                }

                _includeProperties.Add(new IncludeProperty(member, fieldList));
            }

            return this;
        }

        public MyQueryable<T> Include(string navPropertyName)
        {
            var property = _masterEntity.Properties.Single(p => p.Name == navPropertyName);
            if (property != null && property.JoinAble)
            {
                var prop = _includeProperties.SingleOrDefault(p => p.PropertyName == navPropertyName);
                if (prop != null)
                {
                    _includeProperties.Remove(prop);
                }

                _includeProperties.Add(new IncludeProperty(navPropertyName, null));
            }
            return this;
        }

        public MyQueryable<T> Include(string navPropertyName, string[] fields)
        {
            var property = _masterEntity.Properties.Single(p => p.Name == navPropertyName);
            if (property != null && property.JoinAble)
            {
                var prop = _includeProperties.SingleOrDefault(p => p.PropertyName == navPropertyName);
                if (prop != null)
                {
                    _includeProperties.Remove(prop);
                }

                _includeProperties.Add(new IncludeProperty(navPropertyName, fields));
            }
            return this;
        }
        #endregion

        #region Where
        public MyQueryable<T> Where(Expression<Func<T, bool>> expr)
        {
            if (_hasInitWhere)
            {
                throw new ArgumentException("每个查询只能调用一次Where方法");
            }
            _hasInitWhere = true;

            var condition = new QueryConditionResolver<T>(_masterEntity);
            var result = condition.Resolve(expr.Body);
            _where = result.Condition;
            _parameters.AddParameters(result.Parameters);
            _entityCache.AddRange(result.NavPropertyList);
            _whereProperties = result.NavPropertyList.Select(p => p.Name).ToList();

            return this;
        }
        #endregion

        #region OrderBy,ThenOrderBy
        public MyQueryable<T> OrderBy<TProperty>(Expression<Func<T, TProperty>> expression,
            MyDbOrderBy orderBy = MyDbOrderBy.Asc)
        {
            if (expression.Body.NodeType == ExpressionType.MemberAccess)
            {
                _orderBy = GetOrderByString((MemberExpression)expression.Body);
                if (orderBy == MyDbOrderBy.Desc)
                {
                    _orderBy += " DESC";
                }
            }

            return this;
        }

        public MyQueryable<T> ThenOrderBy<TProperty>(Expression<Func<T, TProperty>> expression,
            MyDbOrderBy orderBy = MyDbOrderBy.Asc)
        {
            if (string.IsNullOrWhiteSpace(_orderBy))
            {
                throw new ArgumentNullException(nameof(_orderBy), "排序字段为空，必须先调用OrderBy或OrderByDesc才能调用此方法");
            }
            if (expression.Body.NodeType == ExpressionType.MemberAccess)
            {
                _orderBy += "," + GetOrderByString((MemberExpression)expression.Body);
                if (orderBy == MyDbOrderBy.Desc)
                {
                    _orderBy += " DESC";
                }
            }

            return this;
        }
        #endregion

        #region Select

        public MySelect<TTarget> Select<TTarget>(Expression<Func<T, object>> expression)
        {
            var visitor = new SelectExpressionResolver();
            visitor.Visit(expression);
            _selectProperties.AddRange(visitor.GetPropertyList());
            return new MySelect<TTarget>(_connectionString, GetFields(), GetFrom(), _where, _parameters, _orderBy);
        }

        public MySelect Select(Expression<Func<T, object>> expression)
        {
            var visitor = new SelectExpressionResolver();
            visitor.Visit(expression);
            _selectProperties.AddRange(visitor.GetPropertyList());
            return new MySelect(_connectionString, GetFields(), GetFrom(), _where, _parameters, _orderBy);
        }
        
        #endregion

        #region 输出

        public List<T> ToList()
        {
            var fields = GetFields();
            var from = GetFrom();

            var sqlBuilder = new SqlServerBuilder();
            var sql = sqlBuilder.Select(from, fields, _where, _orderBy);

            var visitor = new SqlDataReaderConverter<T>(_includeProperties.Select(p => p.PropertyName));
            List<T> result;
            using (var conn = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(_parameters.Parameters);
                conn.Open();
                using (var sdr = command.ExecuteReader())
                {
                    result = visitor.ConvertToEntityList(sdr);
                }
            }
            return result;
        }

        public async Task<List<T>> ToListAsync()
        {
            var fields = GetFields();
            var from = GetFrom();

            var sqlBuilder = new SqlServerBuilder();
            var sql = sqlBuilder.Select(from, fields, _where, _orderBy);

            var visitor = new SqlDataReaderConverter<T>(_includeProperties.Select(p => p.PropertyName));
            List<T> result;
            using (var conn = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(_parameters.Parameters);
                conn.Open();
                using (var sdr = await command.ExecuteReaderAsync())
                {
                    result = visitor.ConvertToEntityList(sdr);
                }
            }
            return result;
        }

        public List<T> ToPageList(int pageIndex, int pageSize, out int recordCount)
        {
            var fields = GetFields();
            var from = GetFrom();
            recordCount = 0;

            var sqlBuilder = new SqlServerBuilder();
            var sql = sqlBuilder.PagingSelect(from, fields, _where, _orderBy, pageIndex, pageSize);

            var command = new SqlCommand(sql);
            command.Parameters.AddRange(_parameters.Parameters);
            var param = new SqlParameter("@RecordCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
            command.Parameters.Add(param);

            List<T> result;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                command.Connection = conn;
                using (var sdr = command.ExecuteReader())
                {
                    var handler = new SqlDataReaderConverter<T>(_includeProperties.Select(p => p.PropertyName).ToArray());
                    result = handler.ConvertToEntityList(sdr);
                }
            }

            recordCount = (int)param.Value;
            return result;
        }

        public async Task<PagingResult<T>> ToPageListAsync (int pageIndex, int pageSize)
        {
            var fields = GetFields();
            var from = GetFrom();

            var sqlBuilder = new SqlServerBuilder();
            var sql = sqlBuilder.PagingSelect(from, fields, _where, _orderBy, pageIndex, pageSize);

            var command = new SqlCommand(sql);
            command.Parameters.AddRange(_parameters.Parameters);
            var param = new SqlParameter("@RecordCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
            command.Parameters.Add(param);

            var result = new PagingResult<T>();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                command.Connection = conn;
                using (var sdr = await command.ExecuteReaderAsync())
                {
                    var handler = new SqlDataReaderConverter<T>(_includeProperties.Select(p => p.PropertyName).ToArray());
                    result.Items = handler.ConvertToEntityList(sdr);
                }
            }

            result.RecordCount = (int)param.Value;
            return result;
        }

        public T FirstOrDefault()
        {
            var fields = GetFields();
            var from = GetFrom();

            var sqlBuilder = new SqlServerBuilder();
            var sql = sqlBuilder.Select(from, fields, _where, _orderBy, 1);

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(_parameters.Parameters);
                var sdr = command.ExecuteReader();

                var handler = new SqlDataReaderConverter<T>(_includeProperties.Select(p => p.PropertyName).ToArray());
                return handler.ConvertToEntity(sdr);
            }
        }

        public async Task<T> FirstOrDefaultAsync()
        {
            var fields = GetFields();
            var from = GetFrom();

            var sqlBuilder = new SqlServerBuilder();
            var sql = sqlBuilder.Select(from, fields, _where, _orderBy, 1);

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(_parameters.Parameters);
                using (var sdr = await command.ExecuteReaderAsync())
                {
                    var handler = new SqlDataReaderConverter<T>(_includeProperties.Select(p => p.PropertyName).ToArray());
                    return handler.ConvertToEntity(sdr);
                }
            }
        }

        #endregion

        #region 辅助方法

        /// 把要用到的导航属性的MyEntity缓存到一个List里，
        /// 不需要每次都要到字典中获取
        private MyEntity GetIncludePropertyEntityInfo(Type type)
        {
            var entity = _entityCache.FirstOrDefault(e => e.Name == type.FullName);

            if (entity != null) return entity;

            entity = MyEntityContainer.Get(type);
            _entityCache.Add(entity);
            return entity;
        }

        // 获取Select子句
        private string GetFields()
        {
            if (_selectProperties.Count == 0)
            {
                var masterFields = string.Join(
                    ",",
                    _masterEntity
                        .Properties
                        .Where(p => p.IsMap)
                        .Select(p => $"[{_masterEntity.TableName}].[{p.FieldName}] AS [{p.Name}]")
                );

                if (_includeProperties.Count > 0)
                {
                    var sb = new StringBuilder(masterFields);
                    sb.Append(",");
                    var includeProperties = _includeProperties.OrderBy(i => i);

                    foreach (var property in includeProperties)
                    {
                        var prop = _masterEntity.Properties.Single(p => p.Name == property.PropertyName);
                        var propEntity = GetIncludePropertyEntityInfo(prop.PropertyInfo.PropertyType);
                        if (property.FieldList.Count == 0)
                        {
                            sb.Append(
                                string.Join(",",
                                    propEntity.Properties.Where(p => p.IsMap).Select(p =>
                                        $"[{property.PropertyName}].[{p.FieldName}] AS [{property.PropertyName}__{p.Name}]"))
                            );
                        }
                        else
                        {
                            sb.Append(
                                string.Join(",",
                                    propEntity.Properties.Where(p =>
                                            p.IsMap && property.FieldList.Contains(p.Name))
                                        .Select(p =>
                                            $"[{property.PropertyName}].[{p.FieldName}] AS [{property.PropertyName}__{p.Name}]"))
                            );
                        }
                    }

                    return sb.ToString();
                }

                return masterFields;
            }
            else
            {
                _includeProperties.Clear();
                var sb = new StringBuilder();
                foreach (var property in _selectProperties)
                {
                    if (string.IsNullOrWhiteSpace(property.FieldName))
                    {
                        var prop = _masterEntity.Properties.Single(p => p.Name == property.PropertyName);
                        if (prop != null)
                        {
                            sb.Append($",[{_masterEntity.TableName}].[{prop.FieldName}] AS [{property.MemberName}]");
                        }
                    }
                    else
                    {
                        if (_masterEntity.Properties.Any(p => p.Name == property.PropertyName))
                        {
                            _includeProperties.Add(new IncludeProperty(property.PropertyName, null));
                            var prop = _masterEntity.Properties.Single(p => p.Name == property.PropertyName);
                            var propEntity = GetIncludePropertyEntityInfo(prop.PropertyInfo.PropertyType);

                            var field = propEntity.Properties.Single(p => p.Name == property.FieldName);
                            if (field != null)
                            {
                                sb.Append(
                                    $",[{property.PropertyName}].[{field.FieldName}] AS [{property.MemberName}]");
                            }
                        }
                    }
                }

                return sb.Remove(0, 1).ToString();
            }
        }

        // 获取From子句
        private string GetFrom()
        {
            var masterTable = $"[{_masterEntity.TableName}]";
            var allJoinProperties = _includeProperties.Select(p => p.PropertyName).Concat(_whereProperties).Distinct().ToList();

            if (!allJoinProperties.Any()) return masterTable;
            {
                var sb = new StringBuilder(masterTable);
                foreach (var property in allJoinProperties)
                {
                    var prop = _masterEntity.Properties.Single(p => p.Name == property);
                    if (prop == null) continue;
                    var propEntity = GetIncludePropertyEntityInfo(prop.PropertyInfo.PropertyType);
                    sb.Append($" LEFT JOIN [{propEntity.TableName}] AS [{property}] ON [{_masterEntity.TableName}].[{prop.ForeignKey}]=[{property}].[{prop.MasterKey}]");
                }

                return sb.ToString();
            }

        }

        // 获取OrderBy子句
        private string GetOrderByString(MemberExpression expression)
        {
            expression.GetRootType(out var stack);
            switch (stack.Count)
            {
                case 1:
                {
                    var propName = stack.Pop();
                    var prop = _masterEntity.Properties.Single(p => p.Name == propName);
                    return $"[{_masterEntity.TableName}].[{prop.FieldName}]";
                }
                case 2:
                {
                    var slavePropName = stack.Pop();
                    var propertyName = stack.Pop();

                    var masterProp = _masterEntity.Properties.Single(p => p.Name == propertyName);
                    var slaveEntity = GetIncludePropertyEntityInfo(masterProp.PropertyInfo.PropertyType);
                    var slaveProperty = slaveEntity.Properties.Single(p => p.Name == slavePropName);

                    return $"[{masterProp.Name}].[{slaveProperty.FieldName}]";
                }
                default:
                    return string.Empty;
            }
        }

        public string GetJoinString(MyJoinType joinType)
        {
            switch (joinType)
            {
                case MyJoinType.LeftJoin:
                    return "LEFT JOIN";
                case MyJoinType.InnerJoin:
                    return "INNER JOIN";
                default:
                    throw new ArgumentException("无效的表连接类型", nameof(joinType));
            }
        }

        #endregion
    }

    public class IncludeProperty
    {
        public string PropertyName { get; set; }

        public List<string> FieldList { get; set; }

        public MyJoinType JoinType { get; set; }

        public IncludeProperty(string propertyName, IEnumerable<string> fields,
            MyJoinType joinType = MyJoinType.LeftJoin)
        {
            PropertyName = propertyName;

            FieldList = fields?.ToList() ?? new List<string>();

            JoinType = joinType;
        }
    }
}
