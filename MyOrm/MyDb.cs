using MyOrm.Commons;
using MyOrm.DbParameters;
using MyOrm.Expressions;
using MyOrm.Queryable;
using MyOrm.Reflections;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq.Expressions;

namespace MyOrm
{
    public partial class MyDb
    {
        private readonly string _connectionString;

        private readonly string _prefix;

        public MyDb(string connectionString, string prefix = "@")
        {
            _connectionString = connectionString;
            _prefix = prefix;
        }

        public MyDb()
        {
            if (string.IsNullOrWhiteSpace(MyDbConfiguration.GetConnectionString()))
            {
                throw new Exception("MyOrm尚未初始化");
            }

            _connectionString = MyDbConfiguration.GetConnectionString();
            _prefix = MyDbConfiguration.GetPrefix();
        }

        /// <summary>
        /// 使用默认配置，返回新MyDb实例
        /// </summary>
        /// <returns></returns>
        public static MyDb New()
        {
            return new MyDb();
        }

        /// <summary>
        /// 返回新MyDb实例
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static MyDb New(string connectionString, string prefix = "@")
        {
            return new MyDb(connectionString, prefix);
        }

        #region 查询
        /// <summary>
        /// 返回MyQueryable实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>实体，若记录为空，返回default(T)</returns>
        public MyQueryable<T> Query<T>() where T : class, IEntity, new()
        {
            return new MyQueryable<T>(_connectionString);
        }

        /// <summary>
        /// 根据ID加载一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">实体ID</param>
        /// <returns>实体，若记录为空，返回default(T)</returns>
        public T Load<T>(int id) where T : class, IEntity, new()
        {
            return new MyQueryable<T>(_connectionString).Where(t => t.Id == id).FirstOrDefault();
        }

        /// <summary>
        /// 根据条件加载一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderBy">排序字段</param>
        /// <param name="dbSort">正序或倒序</param>
        /// <returns>实体，若记录为空，返回default(T)</returns>
        public T Load<T>(Expression<Func<T, bool>> where = null,
                         Expression<Func<T, object>> orderBy = null,
                         MyDbOrderBy dbSort = MyDbOrderBy.Asc) where T : class, new()
        {
            var query = new MyQueryable<T>(_connectionString);
            if (where != null)
            {
                query.Where(where);
            }

            if (orderBy != null)
            {
                query.OrderBy(orderBy, dbSort);
            }

            return query.FirstOrDefault();
        }

        /// <summary>
        /// 根据条件加载所有实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderBy">排序字段</param>
        /// <param name="dbSort">正序或倒序</param>
        /// <returns>实体列表</returns>
        public List<T> Fetch<T>(Expression<Func<T, bool>> where = null, Expression<Func<T, object>> orderBy = null,
                                MyDbOrderBy dbSort = MyDbOrderBy.Asc) where T : class, new()
        {
            var query = new MyQueryable<T>(_connectionString);
            if (where != null)
            {
                query.Where(where);
            }

            if (orderBy != null)
            {
                query.OrderBy(orderBy, dbSort);
            }

            return query.ToList();
        }

        /// <summary>
        /// 加载分页列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页条数</param>
        /// <param name="recordCount">记录总数</param>
        /// <param name="where">查询条件</param>
        /// <param name="orderBy">排序字段</param>
        /// <param name="dbSort">正序或倒序</param>
        /// <returns>实体列表，输出记录总数</returns>
        public List<T> PageList<T>(int pageIndex,
            int pageSize,
            out int recordCount,
            Expression<Func<T, bool>> where = null,
            Expression<Func<T, object>> orderBy = null,
            MyDbOrderBy dbSort = MyDbOrderBy.Asc) where T : class, new()
        {
            var query = new MyQueryable<T>(_connectionString);
            if (where != null)
            {
                query.Where(where);
            }

            if (orderBy != null)
            {
                query.OrderBy(orderBy, dbSort);
            }

            return query.ToPageList(pageIndex, pageSize, out recordCount);
        }
        #endregion

        #region 获取数量

        public int GetCount<T>(Expression<Func<T, bool>> expression = null) where T : class, IEntity, new()
        {
            var entityInfo = MyEntityContainer.Get(typeof(T));

            if (expression == null)
            {
                var sql = $"SELECT COUNT(0) FROM [{entityInfo.TableName}]";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var command = new SqlCommand(sql, conn);
                    return (int)command.ExecuteScalar();
                }
            }
            else
            {
                var resolver = new EditConditionResolver<T>(entityInfo);
                var result = resolver.Resolve(expression.Body);
                var condition = result.Condition;
                var parameters = result.Parameters;

                condition = string.IsNullOrWhiteSpace(condition) ? "1=1" : condition;

                var sql = $"SELECT COUNT(0) FROM [{entityInfo.TableName}] WHERE [{condition}]";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var command = new SqlCommand(sql, conn);
                    command.Parameters.AddRange(parameters.Parameters);
                    return (int)command.ExecuteScalar();
                }
            }
        }
        #endregion

        #region 执行SQL语句
        //public List<T> Fetch<T>(string sql, MyDbParameters parameters = null)
        //{

        //}
        #endregion
    }
}
