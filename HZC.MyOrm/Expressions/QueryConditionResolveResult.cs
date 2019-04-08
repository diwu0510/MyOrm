using System.Collections.Generic;
using HZC.MyOrm.DbParameters;
using HZC.MyOrm.Reflections;

namespace HZC.MyOrm.Expressions
{
    public class QueryConditionResolveResult
    {
        public string Condition { get; set; }

        public MyDbParameters Parameters { get; set; } = new MyDbParameters();

        public List<MyEntity> NavPropertyList { get; set; } = new List<MyEntity>();
    }
}
