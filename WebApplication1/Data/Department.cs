using HZC.MyOrm.Attributes;
using HZC.MyOrm.Commons;

namespace WebApplication1
{
    [MyTable("Departments")]
    public class Department : IEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
