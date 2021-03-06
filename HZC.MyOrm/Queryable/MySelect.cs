﻿using HZC.MyOrm.Commons;
using HZC.MyOrm.DbParameters;
using HZC.MyOrm.Mappers;
using HZC.MyOrm.SqlBuilder;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace HZC.MyOrm.Queryable
{
    public class MySelect
    {
        private readonly string _connectionString;
        private readonly string _fields;
        private readonly string _table;
        private readonly string _where;
        private readonly string _orderBy;
        private readonly MyDbParameters _parameters;

        public MySelect(string connectionString, string fields, string table, string where, MyDbParameters dbParameters, string orderBy)
        {
            _fields = fields;
            _table = table;
            _where = where;
            _parameters = dbParameters;
            _orderBy = orderBy;
            _connectionString = connectionString;
        }

        #region 所有数据

        public List<dynamic> ToList()
        {

            var sqlBuilder = new SqlServerBuilder();
            var sql = sqlBuilder.Select(_table, _fields, _where, _orderBy);

            List<dynamic> result;
            var visitor = new SqlDataReaderSelectConverter();
            using (var conn = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(_parameters.Parameters);
                conn.Open();
                using (var sdr = command.ExecuteReader())
                {
                    result = visitor.ConvertToDynamicList(sdr);
                }
            }

            return result;
        }

        public async Task<List<dynamic>> ToListAsync()
        {

            var sqlBuilder = new SqlServerBuilder();
            var sql = sqlBuilder.Select(_table, _fields, _where, _orderBy);

            List<dynamic> result;
            var visitor = new SqlDataReaderSelectConverter();
            using (var conn = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(_parameters.Parameters);
                conn.Open();
                using (var sdr = await command.ExecuteReaderAsync())
                {
                    result = visitor.ConvertToDynamicList(sdr);
                }
            }

            return result;
        }

        #endregion

        #region 分页数据

        public List<dynamic> ToPageList(int pageIndex, int pageSize, out int recordCount)
        {
            var sqlBuilder = new SqlServerBuilder();
            var sql = sqlBuilder.PagingSelect2008(_table, _fields, _where, _orderBy, pageIndex, pageSize);

            var command = new SqlCommand(sql);
            command.Parameters.AddRange(_parameters.Parameters);
            var param = new SqlParameter("@RecordCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
            command.Parameters.Add(param);

            recordCount = 0;
            List<dynamic> result;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                command.Connection = conn;
                using (var sdr = command.ExecuteReader())
                {
                    var handler = new SqlDataReaderSelectConverter();
                    result = handler.ConvertToDynamicList(sdr);
                }
            }

            recordCount = (int)param.Value;
            return result;
        }

        public async Task<PagingResult<dynamic>> ToPageListAsync(int pageIndex, int pageSize)
        {
            var sqlBuilder = new SqlServerBuilder();
            var sql = sqlBuilder.PagingSelect2008(_table, _fields, _where, _orderBy, pageIndex, pageSize);

            var command = new SqlCommand(sql);
            command.Parameters.AddRange(_parameters.Parameters);
            var param = new SqlParameter("@RecordCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
            command.Parameters.Add(param);

            var result = new PagingResult<dynamic>();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                command.Connection = conn;
                using (var sdr = await command.ExecuteReaderAsync())
                {
                    var handler = new SqlDataReaderSelectConverter();
                    result.Items = handler.ConvertToDynamicList(sdr);
                }
            }

            result.RecordCount = (int)param.Value;
            return result;
        }

        #endregion

        #region 第一条数据

        public dynamic FirstOrDefault()
        {
            var sqlBuilder = new SqlServerBuilder();
            var sql = sqlBuilder.Select(_table, _fields, _where, _orderBy, 1);

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(_parameters.Parameters);
                var sdr = command.ExecuteReader();

                var handler = new SqlDataReaderSelectConverter();
                return handler.ConvertToDynamicEntity(sdr);
            }
        }

        public async Task<dynamic> FirstOrDefaultAsync()
        {
            var sqlBuilder = new SqlServerBuilder();
            var sql = sqlBuilder.Select(_table, _fields, _where, _orderBy, 1);

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(_parameters.Parameters);
                var sdr = await command.ExecuteReaderAsync();

                var handler = new SqlDataReaderSelectConverter();
                return handler.ConvertToDynamicEntity(sdr);
            }
        }

        #endregion
    }
}
