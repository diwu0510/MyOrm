using HZC.MyOrm.Commons;
using HZC.MyOrm.Expressions;
using HZC.MyOrm.Reflections;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HZC.MyOrm
{
    public partial class MyDb
    {
        #region 删除

        /// <summary>
        /// 根据ID删除记录，如果支持软删除并且非强制删除，则更新IsDel字段为true，否则，删除记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">要删除的实体ID</param>
        /// <param name="isForce">是否强制删除，默认为false</param>
        /// <returns>受影响的记录数</returns>
        public int Delete<T>(int id, bool isForce = false) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            if (isForce || !entityInfo.IsSoftDelete)
            {
                var sql = $"DELETE [{entityInfo.TableName}] WHERE [{entityInfo.KeyColumn}]={_prefix}Id";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var command = new SqlCommand(sql, conn);
                    command.Parameters.AddWithValue($"{_prefix}Id", id);
                    return command.ExecuteNonQuery();
                }
            }
            else
            {
                var sql = $"UPDATE [{entityInfo.TableName}] SET IsDel=1 WHERE [{entityInfo.KeyColumn}]={_prefix}Id";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var command = new SqlCommand(sql, conn);
                    command.Parameters.AddWithValue($"{_prefix}Id", id);
                    return command.ExecuteNonQuery();
                }
            }
        }

        public async Task<int> DeleteAsync<T>(int id, bool isForce = false) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            if (isForce || !entityInfo.IsSoftDelete)
            {
                var sql = $"DELETE [{entityInfo.TableName}] WHERE [{entityInfo.KeyColumn}]={_prefix}Id";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var command = new SqlCommand(sql, conn);
                    command.Parameters.AddWithValue($"{_prefix}Id", id);
                    return await command.ExecuteNonQueryAsync();
                }
            }
            else
            {
                var sql = $"UPDATE [{entityInfo.TableName}] SET IsDel=1 WHERE [{entityInfo.KeyColumn}]={_prefix}Id";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var command = new SqlCommand(sql, conn);
                    command.Parameters.AddWithValue($"{_prefix}Id", id);
                    return await command.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// 根据ID批量删除记录，如果支持软删除并且非强制删除，则更新IsDel字段为true，否则，删除记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="idList">要删除的ID列表</param>
        /// <param name="isForce">是否强制删除，默认为false</param>
        /// <returns>受影响的记录数</returns>
        public int Delete<T>(IEnumerable<int> idList, bool isForce = false) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            if (isForce || !entityInfo.IsSoftDelete)
            {
                var sql =
                    $"EXEC('DELETE [{entityInfo.TableName}] WHERE [{entityInfo.KeyColumn}] in ('+{_prefix}Ids+')')";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var command = new SqlCommand(sql, conn);
                    command.Parameters.AddWithValue($"{_prefix}Ids", string.Join(",", idList));
                    return command.ExecuteNonQuery();
                }
            }
            else
            {
                var sql = $"EXEC('UPDATE [{entityInfo.TableName}] SET IsDel=1 WHERE [{entityInfo.KeyColumn}] in ('+{_prefix}Ids+')')";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var command = new SqlCommand(sql, conn);
                    command.Parameters.AddWithValue($"{_prefix}Id", idList);
                    return command.ExecuteNonQuery();
                }
            }
        }

        public async Task<int> DeleteAsync<T>(IEnumerable<int> idList, bool isForce = false) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));
            if (isForce || !entityInfo.IsSoftDelete)
            {
                var sql =
                    $"EXEC('DELETE [{entityInfo.TableName}] WHERE [{entityInfo.KeyColumn}] in ('+{_prefix}Ids+')')";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var command = new SqlCommand(sql, conn);
                    command.Parameters.AddWithValue($"{_prefix}Ids", string.Join(",", idList));
                    return await command.ExecuteNonQueryAsync();
                }
            }
            else
            {
                var sql = $"EXEC('UPDATE [{entityInfo.TableName}] SET IsDel=1 WHERE [{entityInfo.KeyColumn}] in ('+{_prefix}Ids+')')";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var command = new SqlCommand(sql, conn);
                    command.Parameters.AddWithValue($"{_prefix}Id", idList);
                    return await command.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// 根据条件删除记录，如果支持软删除并且非强制删除，则更新IsDel字段为true，否则，删除记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">条件，注意不支持导航属性及其子属性</param>
        /// <param name="isForce">是否强制删除</param>
        /// <returns>受影响的记录数</returns>
        public int Delete<T>(Expression<Func<T, bool>> expression, bool isForce) where T : IEntity
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));

            var resolver = new EditConditionResolver<T>(entityInfo);
            var result = resolver.Resolve(expression.Body);
            var condition = result.Condition;
            var parameters = result.Parameters;

            condition = string.IsNullOrWhiteSpace(condition) ? "1=1" : condition;
            string sql;
            if (isForce || !entityInfo.IsSoftDelete)
            {
                sql =
                    $"DELETE [{entityInfo.TableName}] WHERE {condition}";
            }
            else
            {
                sql =
                    $"UPDATE [{entityInfo.TableName}] SET IsDel=1 WHERE {condition}";
            }

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(parameters.Parameters);
                return command.ExecuteNonQuery();
            }
        }

        public async Task<int> DeleteAsync<T>(Expression<Func<T, bool>> expression, bool isForce) where T : IEntity
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));

            var resolver = new EditConditionResolver<T>(entityInfo);
            var result = resolver.Resolve(expression.Body);
            var condition = result.Condition;
            var parameters = result.Parameters;

            condition = string.IsNullOrWhiteSpace(condition) ? "1=1" : condition;
            string sql;
            if (isForce || !entityInfo.IsSoftDelete)
            {
                sql =
                    $"DELETE [{entityInfo.TableName}] WHERE {condition}";
            }
            else
            {
                sql =
                    $"UPDATE [{entityInfo.TableName}] SET IsDel=1 WHERE {condition}";
            }

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(parameters.Parameters);
                return await command.ExecuteNonQueryAsync();
            }
        }
        #endregion
    }
}
