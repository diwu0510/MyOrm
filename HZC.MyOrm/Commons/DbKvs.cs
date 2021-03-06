﻿using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace HZC.MyOrm.Commons
{
    public class DbKvs : List<KeyValuePair<string, object>>
    {
        public static DbKvs New()
        {
            return new DbKvs();
        }

        public DbKvs Add(string key, object value)
        {
            Add(new KeyValuePair<string, object>(key, value));
            return this;
        }

        public List<SqlParameter> ToSqlParameters(string prefix = "@")
        {
            return this.Select(kv => new SqlParameter($"{prefix}{kv.Key}", kv.Value)).ToList();
        }
    }
}
