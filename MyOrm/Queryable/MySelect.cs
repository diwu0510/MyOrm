using MyOrm.DbParameters;
using MyOrm.Mappers;
using MyOrm.SqlBuilder;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace MyOrm.Queryable
{
    public class MySelect<T>
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

        public List<T> ToList()
        {

            var sqlBuilder = new SqlServerBuilder();
            var sql = sqlBuilder.Select(_table, _fields, _where, _orderBy);

            List<T> result;
            var visitor = new SqlDataReaderSelectConverter();
            using (var conn = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddRange(_parameters.Parameters);
                conn.Open();
                using (var sdr = command.ExecuteReader())
                {
                    result = visitor.ConvertToList<T>(sdr);
                }
            }

            return result;
        }

        //public List<dynamic> DynamicList()
        //{
        //    var sqlBuilder = new SqlServerBuilder();
        //    var sql = sqlBuilder.Select(_table, _fields, _where, _orderBy);

        //    var visitor = new SqlDataReaderMapper();
        //    List<dynamic> result;
        //    using (var conn = new SqlConnection(_connectionString))
        //    {
        //        var command = new SqlCommand(sql, conn);
        //        command.Parameters.AddRange(_parameters.Parameters);
        //        conn.Open();
        //        using (var sdr = command.ExecuteReader())
        //        {
        //            result = visitor.ConvertToList(sdr);
        //        }
        //    }

        //    return result;
        //}

        public List<T> ToPageList(int pageIndex, int pageSize, out int recordCount)
        {
            recordCount = 0;

            var sqlBuilder = new SqlServerBuilder();
            var sql = sqlBuilder.PagingSelect2008(_table, _fields, _where, _orderBy, pageIndex, pageSize);

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
                    var handler = new SqlDataReaderSelectConverter();
                    result = handler.ConvertToList<T>(sdr);
                }
            }

            recordCount = (int)param.Value;
            return result;
        }

        //public List<dynamic> ToPageListDynamic(int pageIndex, int pageSize, out int recordCount)
        //{
        //    recordCount = 0;

        //    var sqlBuilder = new SqlServerBuilder();
        //    var sql = sqlBuilder.PagingSelect2008(_table, _fields, _where, _orderBy, pageIndex, pageSize);

        //    var command = new SqlCommand(sql);
        //    command.Parameters.AddRange(_parameters.Parameters);
        //    var param = new SqlParameter("@RecordCount", SqlDbType.Int) {Direction = ParameterDirection.Output};
        //    command.Parameters.Add(param);

        //    List<dynamic> result;

        //    using (var conn = new SqlConnection(_connectionString))
        //    {
        //        conn.Open();
        //        command.Connection = conn;
        //        using (var sdr = command.ExecuteReader())
        //        {    
        //            var handler = new SqlDataReaderMapper();
        //            result = handler.ConvertToList(sdr);
        //        }
        //    }

        //    recordCount = (int) param.Value;
        //    return result;
        //}

        public T FirstOrDefault()
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
                return handler.ConvertToEntity<T>(sdr);
            }
        }

        //public dynamic FirstOrDefaultDynamic()
        //{
        //    var sqlBuilder = new SqlServerBuilder();
        //    var sql = sqlBuilder.Select(_table, _fields, _where, _orderBy, 1);

        //    using (var conn = new SqlConnection(_connectionString))
        //    {
        //        conn.Open();
        //        var command = new SqlCommand(sql, conn);
        //        command.Parameters.AddRange(_parameters.Parameters);
        //        var sdr = command.ExecuteReader();

        //        var handler = new SqlDataReaderMapper();
        //        return handler.ConvertToEntity(sdr);
        //    }
        //}
    }
}
