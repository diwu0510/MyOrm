using HZC.MyOrm.Commons;
using HZC.MyOrm.DbParameters;
using HZC.MyOrm.Expressions;
using HZC.MyOrm.Reflections;
using HZC.MyOrm.SqlBuilder;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HZC.MyOrm
{
    public partial class MyDb
    {
        /// <summary>
        /// 创建一个实体，新的记录Id将绑定到entity的Id属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">要创建的实体</param>
        /// <returns>新生成记录的ID，若失败返回0</returns>
        public int Insert<T>(T entity) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));

            var sqlBuilder = new SqlServerBuilder();
            var sql = sqlBuilder.Insert(entityInfo);

            var parameters = new MyDbParameters();
            parameters.Add(entity);

            var command = new SqlCommand(sql);
            command.Parameters.AddRange(parameters.Parameters);

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                command.Connection = conn;
                var obj = command.ExecuteScalar();
                if (obj != DBNull.Value)
                {
                    entity.Id = Convert.ToInt32(obj);
                }
                return entity.Id;
            }
        }

        public async Task<int> InsertAsync<T>(T entity) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));

            var sqlBuilder = new SqlServerBuilder();
            var sql = sqlBuilder.Insert(entityInfo);

            var parameters = new MyDbParameters();
            parameters.Add(entity);

            var command = new SqlCommand(sql);
            command.Parameters.AddRange(parameters.Parameters);

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                command.Connection = conn;
                var obj = await command.ExecuteScalarAsync();
                if (obj != DBNull.Value)
                {
                    entity.Id = Convert.ToInt32(obj);
                }
                return entity.Id;
            }
        }

        /// <summary>
        /// 如果不满足条件则创建一个实体，
        /// 如限制用户名不能重复 InsertIfNotExist(user, u => u.Name == user.Name)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">要创建的实体</param>
        /// <param name="where">条件</param>
        /// <returns>新生成记录的ID，若失败返回0</returns>
        public int InsertIfNotExists<T>(T entity, Expression<Func<T, bool>> where) where T : class, IEntity, new()
        {
            if (where == null)
            {
                return Insert(entity);
            }

            var entityInfo = MyEntityContainer.Get(typeof(T));
            var resolver = new EditConditionResolver<T>(entityInfo);
            var result = resolver.Resolve(@where.Body);
            var condition = result.Condition;
            var parameters = result.Parameters;
            parameters.Add(entity);

            condition = string.IsNullOrWhiteSpace(condition) ? "1=1" : condition;

            var sqlBuilder = new SqlServerBuilder();
            var sql = sqlBuilder.InsertIfNotExists(entityInfo, condition);
            var command = new SqlCommand(sql);
            command.Parameters.AddRange(parameters.Parameters);

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                command.Connection = conn;
                var obj = command.ExecuteScalar();
                if (obj != DBNull.Value)
                {
                    entity.Id = Convert.ToInt32(obj);
                }
                return entity.Id;
            }
        }

        public async Task<int> InsertIfNotExistsAsync<T>(T entity, Expression<Func<T, bool>> where) where T : class, IEntity, new()
        {
            if (where == null)
            {
                return Insert(entity);
            }

            var entityInfo = MyEntityContainer.Get(typeof(T));
            var resolver = new EditConditionResolver<T>(entityInfo);
            var result = resolver.Resolve(@where.Body);
            var condition = result.Condition;
            var parameters = result.Parameters;
            parameters.Add(entity);

            condition = string.IsNullOrWhiteSpace(condition) ? "1=1" : condition;

            var sqlBuilder = new SqlServerBuilder();
            var sql = sqlBuilder.InsertIfNotExists(entityInfo, condition);
            var command = new SqlCommand(sql);
            command.Parameters.AddRange(parameters.Parameters);

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                command.Connection = conn;
                var obj = await command.ExecuteScalarAsync();
                if (obj != DBNull.Value)
                {
                    entity.Id = Convert.ToInt32(obj);
                }
                return entity.Id;
            }
        }

        /// <summary>
        /// 批量创建实体，注意此方法效率不高
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityList">实体列表</param>
        /// <returns>受影响的记录数</returns>
        public int Insert<T>(List<T> entityList) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));

            var sqlBuilder = new SqlServerBuilder();
            var sql = sqlBuilder.Insert(entityInfo);

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
                                var parameters = new MyDbParameters();
                                parameters.Add(entity);
                                command.Parameters.AddRange(parameters.Parameters);
                                var obj = command.ExecuteScalar();
                                if (obj != DBNull.Value)
                                {
                                    entity.Id = Convert.ToInt32(obj);
                                }
                                count++;
                            }
                        }
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        count = 0;
                    }
                }
            }

            return count;
        }

        public async Task<int> InsertAsync<T>(List<T> entityList) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));

            var sqlBuilder = new SqlServerBuilder();
            var sql = sqlBuilder.Insert(entityInfo);

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
                                var parameters = new MyDbParameters();
                                parameters.Add(entity);
                                command.Parameters.AddRange(parameters.Parameters);
                                var obj = await command.ExecuteScalarAsync();
                                if (obj != DBNull.Value)
                                {
                                    entity.Id = Convert.ToInt32(obj);
                                    count++;
                                }
                            }
                        }
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        count = 0;
                    }
                }
            }

            return count;
        }
    }
}
