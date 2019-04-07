using Dapper;
using System.Collections.Generic;
using MyOrm.DbParameters;
using MyOrm.Reflections;

namespace MyOrm.Expressions
{
    public class QueryConditionResolveResult
    {
        public string Condition { get; set; }

        public MyDbParameters Parameters { get; set; } = new MyDbParameters();

        public List<MyEntity> NavPropertyList { get; set; } = new List<MyEntity>();
    }
}
