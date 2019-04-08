using System.ComponentModel.DataAnnotations;
using HZC.MyOrm.Attributes;
using HZC.MyOrm.Commons;

namespace WebApplication1
{
    [MyTable("Base_AppUsers")]
    public class AppUser : IEntity
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Display(Name = "用户编号")]
        [Required(ErrorMessage = "用户编号不能为空")]
        public string No { get; set; }

        [Display(Name = "用户姓名")]
        [Required(ErrorMessage = "用户姓名不能为空")]
        public string Name { get; set; }

        [Display(Name = "是部门主管")]
        public bool IsMaster { get; set; }

        [Display(Name = "是财务")]
        public bool IsFinance { get; set; }
        
        [Required(ErrorMessage = "部门不能为空")]
        public int DepartmentId { get; set; }

        [Display(Name = "是否删除")]
        [MyColumn(UpdateIgnore = true)]
        public bool IsDelete { get; set; } = false;

        [Display(Name = "部门")]
        public Department Department { get; set; }
    }
}
