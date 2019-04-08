using System.Collections.Generic;
using System.Linq.Expressions;

namespace HZC.MyOrm.Commons
{
    public class PagingResult<T>
    {
        public int RecordCount { get; set; }

        public List<T> Items { get; set; } = new List<T>();
    }
}
