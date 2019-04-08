using System.Data;

namespace HZC.MyOrm.DbParameters
{
    public class MyDbParameter
    {
        public string Name { get; set; }

        public object Value { get; set; }

        public ParameterDirection? Direction { get; set; }
    }
}
