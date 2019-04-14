using System;
using HZC.MyOrm.DbParameters;
using HZC.MyOrm.Mappers;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace HZC.MyOrm
{
    public static class SqlConnectionExtensions
    {
        private static MyDbParameters ConvertToDbParameter(object obj)
        {
            var myDbParameters = new MyDbParameters();

            if (obj is SqlParameter parameter)
            {
                myDbParameters.Add(parameter);
            }
            else if (obj is IEnumerable<SqlParameter> enumerable)
            {
                myDbParameters.Add(enumerable);
            }
            else if (obj is MyDbParameters dbParameters)
            {
                myDbParameters.Add(dbParameters.Parameters);
            }
            else
            {
                myDbParameters.Add(obj);
            }

            return myDbParameters;
        }

        public static List<T> Fetch<T>(this SqlConnection conn, string sql, object parameters = null, SqlTransaction trans = null)
        {
            try
            {
                var command = new SqlCommand(sql, conn, trans);
                if (parameters != null)
                {
                    command.Parameters.AddRange(ConvertToDbParameter(parameters).Parameters);
                }

                var mapper = new SqlDataReaderSelectConverter();
                List<T> result;

                using (var sdr = command.ExecuteReader())
                {
                    result = mapper.ConvertToList<T>(sdr);
                }

                return result;
            }
            catch (Exception)
            {
                trans?.Rollback();
            }
            finally
            {
                conn.Close();
            }
            return new List<T>();
        }

        public static async Task<List<T>> FetchAsync<T>(this SqlConnection conn, string sql, object parameters = null, SqlTransaction trans = null)
        {
            try
            {
                var command = new SqlCommand(sql, conn, trans);
                if (parameters != null)
                {
                    command.Parameters.AddRange(ConvertToDbParameter(parameters).Parameters);
                }

                var mapper = new SqlDataReaderSelectConverter();
                List<T> result;

                using (var sdr = await command.ExecuteReaderAsync())
                {
                    result = mapper.ConvertToList<T>(sdr);
                }

                return result;
            }
            catch (Exception)
            {
                trans?.Rollback();
            }
            finally
            {
                conn.Close();
            }
            return new List<T>();
        }
        
        public static List<dynamic> Fetch(this SqlConnection conn, string sql, object parameters = null, SqlTransaction trans = null)
        {
            try
            {
                var command = new SqlCommand(sql, conn, trans);
                if (parameters != null)
                {
                    command.Parameters.AddRange(ConvertToDbParameter(parameters).Parameters);
                }

                var mapper = new SqlDataReaderSelectConverter();
                List<dynamic> result;

                using (var sdr = command.ExecuteReader())
                {
                    result = mapper.ConvertToDynamicList(sdr);
                }

                return result;
            }
            catch (Exception)
            {
                trans?.Rollback();
            }
            finally
            {
                conn.Close();
            }
            return new List<dynamic>();
        }



        public static async Task<List<dynamic>> FetchAsync(this SqlConnection conn, string sql, object parameters = null, SqlTransaction trans = null)
        {
            try
            {
                var command = new SqlCommand(sql, conn, trans);
                if (parameters != null)
                {
                    command.Parameters.AddRange(ConvertToDbParameter(parameters).Parameters);
                }

                var mapper = new SqlDataReaderSelectConverter();
                List<dynamic> result;

                using (var sdr = await command.ExecuteReaderAsync())
                {
                    result = mapper.ConvertToDynamicList(sdr);
                }

                return result;
            }
            catch (Exception)
            {
                trans?.Rollback();
            }
            finally
            {
                conn.Close();
            }
            return new List<dynamic>();
        }

        public static T SingleOrDefault<T>(this SqlConnection conn, string sql, object parameters = null, SqlTransaction trans = null)
        {
            try
            {
                var command = new SqlCommand(sql, conn, trans);
                if (parameters != null)
                {
                    command.Parameters.AddRange(ConvertToDbParameter(parameters).Parameters);
                }

                var mapper = new SqlDataReaderSelectConverter();
                T result;

                using (var sdr = command.ExecuteReader())
                {
                    result = mapper.ConvertToEntity<T>(sdr);
                }

                return result;
            }
            catch (Exception)
            {
                trans?.Rollback();
            }
            finally
            {
                conn.Close();
            }

            return default(T);
        }

        public static async Task<T> SingleOrDefaultAsync<T>(this SqlConnection conn, string sql, object parameters = null, SqlTransaction trans = null)
        {
            try
            {
                var command = new SqlCommand(sql, conn, trans);
                if (parameters != null)
                {
                    command.Parameters.AddRange(ConvertToDbParameter(parameters).Parameters);
                }

                var mapper = new SqlDataReaderSelectConverter();
                T result;

                using (var sdr = await command.ExecuteReaderAsync())
                {
                    result = mapper.ConvertToEntity<T>(sdr);
                }

                return result;
            }
            catch (Exception)
            {
                trans?.Rollback();
            }
            finally
            {
                conn.Close();
            }

            return default(T);
        }

        public static T ExecuteScalar<T>(this SqlConnection conn, string sql, object parameters = null, SqlTransaction trans = null)
        {
            try
            {
                var command = new SqlCommand(sql, conn, trans);
                if (parameters != null)
                {
                    command.Parameters.AddRange(ConvertToDbParameter(parameters).Parameters);
                }

                var obj = command.ExecuteScalar();
                if (obj != DBNull.Value)
                {
                    return (T)obj;
                }
            }
            catch (Exception)
            {
                trans?.Rollback();
            }
            finally
            {
                conn.Close();
            }

            return default(T);
        }

        public static async Task<T> ExecuteScalarAsync<T>(this SqlConnection conn, string sql, object parameters = null, SqlTransaction trans = null)
        {
            try
            {
                var command = new SqlCommand(sql, conn, trans);
                if (parameters != null)
                {
                    command.Parameters.AddRange(ConvertToDbParameter(parameters).Parameters);
                }

                var obj = await command.ExecuteScalarAsync();
                if (obj != DBNull.Value)
                {
                    return (T) obj;
                }
            }
            catch (Exception)
            {
                trans?.Rollback();
            }
            finally
            {
                conn.Close();
            }

            return default(T);
        }

        public static int Execute(this SqlConnection conn, string sql, object parameters = null, SqlTransaction trans = null)
        {
            try
            {
                conn.Open();
                var command = new SqlCommand(sql, conn, trans);
                if (parameters != null)
                {
                    command.Parameters.AddRange(ConvertToDbParameter(parameters).Parameters);
                }

                return command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                trans?.Rollback();
            }
            finally
            {
                conn.Close();
            }

            return 0;
        }

        public static async Task<int> ExecuteAsync(this SqlConnection conn, string sql, object parameters = null, SqlTransaction trans = null)
        {
            try
            {
                conn.Open();
                var command = new SqlCommand(sql, conn, trans);
                if (parameters != null)
                {
                    command.Parameters.AddRange(ConvertToDbParameter(parameters).Parameters);
                }

                return await command.ExecuteNonQueryAsync();
            }
            catch (Exception)
            {
                trans?.Rollback();
            }
            finally
            {
                conn.Close();
            }

            return 0;
        }
    }
}
