using HZC.MyOrm.Commons;
using HZC.MyOrm.DbParameters;
using HZC.MyOrm.Expressions;
using HZC.MyOrm.Reflections;
using HZC.MyOrm.SqlBuilder;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HZC.MyOrm
{
    public partial class MyDb
    {
        /// <summary>
        /// 更新一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">要更新的实体</param>
        /// <returns>受影响的记录数</returns>
        public int Update<T>(T entity) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));

            var sqlBuilder = new SqlServerBuilder();
            var sql = sqlBuilder.Update(entityInfo, "");

            var parameters = new MyDbParameters();
            parameters.Add(entity);

            var command = new SqlCommand(sql);
            command.Parameters.AddRange(parameters.Parameters);

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                command.Connection = conn;
                return command.ExecuteNonQuery();
            }
        }

        public async Task<int> UpdateAsync<T>(T entity) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));

            var sqlBuilder = new SqlServerBuilder();
            var sql = sqlBuilder.Update(entityInfo, "");

            var parameters = new MyDbParameters();
            parameters.Add(entity);

            var command = new SqlCommand(sql);
            command.Parameters.AddRange(parameters.Parameters);

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                command.Connection = conn;
                return await command.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// 更新多个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityList">要更新的实体列表</param>
        /// <returns>受影响的记录数</returns>
        public int Update<T>(List<T> entityList) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));

            var sqlBuilder = new SqlServerBuilder();
            var sql = sqlBuilder.Update(entityInfo, "");

            var count = 0;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (var entity in entityList)
                        {
                            using (var command = new SqlCommand(sql, conn, trans))
                            {
                                var param = new MyDbParameters();
                                param.Add(entity);
                                command.Parameters.AddRange(param.Parameters);
                                count += command.ExecuteNonQuery();
                            }
                        }
                        trans.Commit();
                    }
                    catch (Exception)
                    {
                        trans.Rollback();
                        count = 0;
                    }
                }
            }

            return count;
        }

        public async Task<int> UpdateAsync<T>(List<T> entityList) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));

            var sqlBuilder = new SqlServerBuilder();
            var sql = sqlBuilder.Update(entityInfo, "");

            var count = 0;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (var entity in entityList)
                        {
                            using (var command = new SqlCommand(sql, conn, trans))
                            {
                                var param = new MyDbParameters();
                                param.Add(entity);
                                command.Parameters.AddRange(param.Parameters);
                                count += await command.ExecuteNonQueryAsync();
                            }
                        }
                        trans.Commit();
                    }
                    catch (Exception)
                    {
                        trans.Rollback();
                        count = 0;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// 如果不存在，则更新
        /// 如：UpdateIfNotExists(user, u=>u.Name == user.Name && u.Id != user.Id)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="where"></param>
        /// <returns>受影响的记录数</returns>
        public int UpdateIfNotExits<T>(T entity, Expression<Func<T, bool>> where)
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));

            var resolver = new EditConditionResolver<T>(entityInfo);
            var result = resolver.Resolve(where.Body);
            var condition = result.Condition;
            var parameters = result.Parameters;
            parameters.Add(entity);

            condition = string.IsNullOrWhiteSpace(condition) ? "1=1" : condition;

            var sqlBuilder = new SqlServerBuilder();
            var sql = sqlBuilder.Update(entityInfo, "");
            sql += $" AND NOT EXISTS (SELECT 1 FROM [{entityInfo.TableName}] WHERE {condition})";

            var command = new SqlCommand(sql);
            command.Parameters.AddRange(parameters.Parameters);

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                command.Connection = conn;
                return command.ExecuteNonQuery();
            }
        }

        public async Task<int> UpdateIfNotExitsAsync<T>(T entity, Expression<Func<T, bool>> where)
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));

            var resolver = new EditConditionResolver<T>(entityInfo);
            var result = resolver.Resolve(where.Body);
            var condition = result.Condition;
            var parameters = result.Parameters;
            parameters.Add(entity);

            condition = string.IsNullOrWhiteSpace(condition) ? "1=1" : condition;

            var sqlBuilder = new SqlServerBuilder();
            var sql = sqlBuilder.Update(entityInfo, "");
            sql += $" AND NOT EXISTS (SELECT 1 FROM [{entityInfo.TableName}] WHERE {condition})";

            var command = new SqlCommand(sql);
            command.Parameters.AddRange(parameters.Parameters);

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                command.Connection = conn;
                return await command.ExecuteNonQueryAsync();
            }
        }

        #region 扩展
        /// <summary>
        /// 通过Id修改指定列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">实体ID</param>
        /// <param name="kvs">属性和值的键值对。用法 DbKvs.New().Add("属性名", 值)</param>
        /// <returns>受影响的记录数</returns>
        public int Update<T>(int id, DbKvs kvs)
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var setProperties = kvs.Where(kv => kv.Key != "Id").Select(kv => kv.Key);
            var includeProperties = entityInfo.Properties.Where(p => setProperties.Contains(p.Name)).ToList();
            if (includeProperties.Count == 0)
            {
                return 0;
            }

            var sql =
                $"UPDATE [{entityInfo.TableName}] SET {string.Join(",", includeProperties.Select(p => $"{p.FieldName}=@{p.Name}"))} WHERE Id=@Id";
            var parameters = kvs.ToSqlParameters();
            parameters.Add(new SqlParameter("@Id", id));
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(parameters.ToArray());
                return command.ExecuteNonQuery();
            }
        }

        public async Task<int> UpdateAsync<T>(int id, DbKvs kvs)
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var setProperties = kvs.Where(kv => kv.Key != "Id").Select(kv => kv.Key);
            var includeProperties = entityInfo.Properties.Where(p => setProperties.Contains(p.Name)).ToList();
            if (includeProperties.Count == 0)
            {
                return 0;
            }

            var sql =
                $"UPDATE [{entityInfo.TableName}] SET {string.Join(",", includeProperties.Select(p => $"{p.FieldName}=@{p.Name}"))} WHERE Id=@Id";
            var parameters = kvs.ToSqlParameters();
            parameters.Add(new SqlParameter("@Id", id));
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(parameters.ToArray());
                return await command.ExecuteNonQueryAsync();
            }
        }

        public int Update<T>(int id, Expression<Func<T, object>> expression)
        {
            var kvs = ObjectExpressionResolver.Resolve(expression.Body);
            return Update<T>(id, kvs);
        }

        public async Task<int> UpdateAsync<T>(int id, Expression<Func<T, object>> expression)
        {
            var kvs = ObjectExpressionResolver.Resolve(expression.Body);
            return await UpdateAsync<T>(id, kvs);
        }

        /// <summary>
        /// 通过查询条件修改指定列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="kvs">属性和值的键值对。用法 DbKvs.New().Add("属性名", 值)</param>
        /// <param name="expression">查询条件，注意：不支持导航属性，如 "student => student.School.Id > 0" 将无法解析</param>
        /// <returns>受影响的记录数</returns>
        public int Update<T>(Expression<Func<T, bool>> expression, DbKvs kvs)
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var setProperties = kvs.Where(kv => kv.Key != "Id").Select(kv => kv.Key);
            var includeProperties = entityInfo.Properties.Where(p => setProperties.Contains(p.Name)).ToList();
            if (includeProperties.Count == 0)
            {
                return 0;
            }

            string sql;
            List<SqlParameter> parameters;
            if (expression == null)
            {
                sql =
                    $"UPDATE [{entityInfo.TableName}] SET {string.Join(",", includeProperties.Select(p => $"{p.FieldName}=@{p.Name}"))} WHERE Id=@Id";
                parameters = kvs.ToSqlParameters();
            }
            else
            {
                var resolver = new EditConditionResolver<T>(entityInfo);
                var result = resolver.Resolve(expression.Body);
                var where = result.Condition;
                var whereParameters = result.Parameters;

                parameters = kvs.ToSqlParameters();
                parameters.AddRange(whereParameters.Parameters);

                where = string.IsNullOrWhiteSpace(where) ? "1=1" : where;

                sql =
                    $"UPDATE [{entityInfo.TableName}] SET {string.Join(",", includeProperties.Select(p => $"{p.FieldName}=@{p.Name}"))} WHERE {where}";
            }

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(parameters.ToArray());
                return command.ExecuteNonQuery();
            }
        }

        public async Task<int> UpdateAsync<T>(Expression<Func<T, bool>> expression, DbKvs kvs)
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var setProperties = kvs.Where(kv => kv.Key != "Id").Select(kv => kv.Key);
            var includeProperties = entityInfo.Properties.Where(p => setProperties.Contains(p.Name)).ToList();
            if (includeProperties.Count == 0)
            {
                return 0;
            }

            string sql;
            List<SqlParameter> parameters;
            if (expression == null)
            {
                sql =
                    $"UPDATE [{entityInfo.TableName}] SET {string.Join(",", includeProperties.Select(p => $"{p.FieldName}=@{p.Name}"))} WHERE Id=@Id";
                parameters = kvs.ToSqlParameters();
            }
            else
            {
                var resolver = new EditConditionResolver<T>(entityInfo);
                var result = resolver.Resolve(expression.Body);
                var where = result.Condition;
                var whereParameters = result.Parameters;

                parameters = kvs.ToSqlParameters();
                parameters.AddRange(whereParameters.Parameters);

                where = string.IsNullOrWhiteSpace(where) ? "1=1" : where;

                sql =
                    $"UPDATE [{entityInfo.TableName}] SET {string.Join(",", includeProperties.Select(p => $"{p.FieldName}=@{p.Name}"))} WHERE {where}";
            }

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(parameters.ToArray());
                return await command.ExecuteNonQueryAsync();
            }
        }

        public int Update<T>(Expression<Func<T, bool>> expression, Expression<Func<T, object>> properties)
        {
            var kvs = ObjectExpressionResolver.Resolve(properties.Body);
            return Update<T>(expression, kvs);
        }

        public async Task<int> UpdateAsync<T>(Expression<Func<T, bool>> expression, Expression<Func<T, object>> properties)
        {
            var kvs = ObjectExpressionResolver.Resolve(properties.Body);
            return await UpdateAsync<T>(expression, kvs);
        }

        /// <summary>
        /// 修改实体的指定属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">要修改的实体</param>
        /// <param name="includes">要修改的属性名称，注意：是实体的属性名而不是数据库字段名</param>
        /// <param name="ignoreAttribute">是否忽略实体的UpdateIgnore描述。默认为true，既includes中包含的所有属性都会被修改</param>
        /// <returns>受影响的记录数</returns>
        public int UpdateInclude<T>(T entity, IEnumerable<string> includes, bool ignoreAttribute = true) where T : IEntity
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var includeProperties = entityInfo.Properties.Where(p => includes.Contains(p.Name) && p.Name != "Id").ToList();

            if (!ignoreAttribute)
            {
                includeProperties = includeProperties.Where(p => !p.UpdateIgnore).ToList();
            }

            if (includeProperties.Count == 0)
            {
                return 0;
            }

            var sql =
                $"UPDATE [{entityInfo.TableName}] SET {string.Join(",", includeProperties.Select(p => $"{p.FieldName}=@{p.Name}"))} WHERE Id=@Id";
            var parameters = new List<SqlParameter> { new SqlParameter("@Id", entity.Id) };

            foreach (var property in includeProperties)
            {
                parameters.Add(new SqlParameter($"@{property.Name}", ResolveParameterValue(property.PropertyInfo.GetValue(entity))));
            }

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(parameters.ToArray());
                return command.ExecuteNonQuery();
            }
        }

        public async Task<int> UpdateIncludeAsync<T>(T entity, IEnumerable<string> includes, bool ignoreAttribute = true) where T : IEntity
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var includeProperties = entityInfo.Properties.Where(p => includes.Contains(p.Name) && p.Name != "Id").ToList();

            if (!ignoreAttribute)
            {
                includeProperties = includeProperties.Where(p => !p.UpdateIgnore).ToList();
            }

            if (includeProperties.Count == 0)
            {
                return 0;
            }

            var sql =
                $"UPDATE [{entityInfo.TableName}] SET {string.Join(",", includeProperties.Select(p => $"{p.FieldName}=@{p.Name}"))} WHERE Id=@Id";
            var parameters = new List<SqlParameter> { new SqlParameter("@Id", entity.Id) };

            foreach (var property in includeProperties)
            {
                parameters.Add(new SqlParameter($"@{property.Name}", ResolveParameterValue(property.PropertyInfo.GetValue(entity))));
            }

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(parameters.ToArray());
                return await command.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// 修改实体的指定属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">要修改的实体</param>
        /// <param name="expression">要修改的属性，注意不支持导航属性及其子属性</param>
        /// <param name="ignoreAttribute">是否忽略实体的UpdateIgnore描述。默认为true，既includes中包含的所有属性都会被修改</param>
        /// <returns>受影响的记录数</returns>
        public int UpdateInclude<T>(T entity, Expression<Func<T, object>> expression, bool ignoreAttribute = true) where T : IEntity
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var visitor = new ObjectMemberVisitor();
            visitor.Visit(expression);
            var include = visitor.GetPropertyList();
            return UpdateInclude(entity, include, ignoreAttribute);
        }

        public async Task<int> UpdateIncludeAsync<T>(T entity, Expression<Func<T, object>> expression, bool ignoreAttribute = true) where T : IEntity
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var visitor = new ObjectMemberVisitor();
            visitor.Visit(expression);
            var include = visitor.GetPropertyList();
            return await UpdateIncludeAsync(entity, include, ignoreAttribute);
        }

        /// <summary>
        /// 修改实体除指定属性外的其他属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">要修改的实体</param>
        /// <param name="ignore">要忽略的属性，注意：是实体的属性名而不是数据表的列名</param>
        /// <param name="ignoreAttribute">是否忽略实体的UpdateIgnore描述。默认为true，既includes中包含的所有属性都会被修改</param>
        /// <returns>受影响的记录数</returns>
        public int UpdateIgnore<T>(T entity, IEnumerable<string> ignore, bool ignoreAttribute = true) where T : IEntity
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var includeProperties = entityInfo.Properties.Where(p => !ignore.Contains(p.Name) && p.Name != "Id").ToList();

            if (!ignoreAttribute)
            {
                includeProperties = includeProperties.Where(p => !p.UpdateIgnore).ToList();
            }

            if (!includeProperties.Any())
            {
                return 0;
            }

            var sql =
                $"UPDATE [{entityInfo.TableName}] SET {string.Join(",", includeProperties.Select(p => $"{p.FieldName}=@{p.Name}"))} WHERE Id=@Id";
            var parameters = new List<SqlParameter> { new SqlParameter("@Id", entity.Id) };

            foreach (var property in includeProperties)
            {
                parameters.Add(new SqlParameter($"@{property.Name}", ResolveParameterValue(property.PropertyInfo.GetValue(entity))));
            }

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(parameters.ToArray());
                return command.ExecuteNonQuery();
            }
        }

        public async Task<int> UpdateIgnoreAsync<T>(T entity, IEnumerable<string> ignore, bool ignoreAttribute = true) where T : IEntity
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var includeProperties = entityInfo.Properties.Where(p => !ignore.Contains(p.Name) && p.Name != "Id").ToList();

            if (!ignoreAttribute)
            {
                includeProperties = includeProperties.Where(p => !p.UpdateIgnore).ToList();
            }

            if (!includeProperties.Any())
            {
                return 0;
            }

            var sql =
                $"UPDATE [{entityInfo.TableName}] SET {string.Join(",", includeProperties.Select(p => $"{p.FieldName}=@{p.Name}"))} WHERE Id=@Id";
            var parameters = new List<SqlParameter> { new SqlParameter("@Id", entity.Id) };

            foreach (var property in includeProperties)
            {
                parameters.Add(new SqlParameter($"@{property.Name}", ResolveParameterValue(property.PropertyInfo.GetValue(entity))));
            }

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(parameters.ToArray());
                return await command.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">要修改的实体</param>
        /// <param name="expression">要修改的属性，注意不支持导航属性及其子属性</param>
        /// <param name="ignoreAttribute">是否忽略实体的UpdateIgnore描述。默认为true，既includes中包含的所有属性都会被修改</param>
        /// <returns>受影响的记录数</returns>
        public int UpdateIgnore<T>(T entity, Expression<Func<T, object>> expression, bool ignoreAttribute = true) where T : IEntity
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var visitor = new ObjectMemberVisitor();
            visitor.Visit(expression);
            var include = visitor.GetPropertyList();
            return UpdateIgnore(entity, include, ignoreAttribute);
        }

        public async Task<int> UpdateIgnoreAsync<T>(T entity, Expression<Func<T, object>> expression, bool ignoreAttribute = true) where T : IEntity
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            var visitor = new ObjectMemberVisitor();
            visitor.Visit(expression);
            var include = visitor.GetPropertyList();
            return await UpdateIgnoreAsync(entity, include, ignoreAttribute);
        }
        #endregion

        private object ResolveParameterValue(object val)
        {
            if (val is null)
            {
                val = DBNull.Value;
            }

            return val;
        }
    }
}
