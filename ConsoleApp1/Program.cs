using HZC.MyOrm.Queryable;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using HZC.MyOrm.Expressions;
using System.Collections;
using System.Collections.Generic;
using HZC.MyOrm;
using Newtonsoft.Json;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            #region 查询效率

            {
                //var sw = new Stopwatch();

                //for (var i = 0; i < 20; i++)
                //{
                //    sw.Restart();
                //    var db = new MyQueryable<Student>("Data Source=.;Database=Taoxue.Order;User Id=sa;Password=790825");
                //    var list = db
                //        .Where(s => !s.IsDel && s.School.Id == 2 && s.CreateAt > DateTime.Today.AddYears(-1))
                //        .Select<StudentDto>(s => new StudentDto
                //        {
                //            Id = s.Id,
                //            Name = s.StudentName,
                //            SchoolName = s.School.SchoolName,
                //            Card = s.Card,
                //            Mobile = s.Mobile,
                //            Birthday = s.Birthday
                //        });

                //    var result = list.ToList();
                //    sw.Stop();

                //    Console.WriteLine($"HZC.MyOrm 第 {i + 1} 次用时：{sw.ElapsedMilliseconds}；数据总量：{result.Count()}");
                //}
            }

            #endregion

            #region 测试Insert

            ////var school = new School
            ////{
            ////    SchoolType = "机构",
            ////    SchoolName = "连幼苗苗园",
            ////    Owner = "123",
            ////    CreateAt = DateTime.Now,
            ////    UpdateAt = DateTime.Now,
            ////    CreateBy = "123",
            ////    UpdateBy = "123"
            ////};

            //var db = MyDb.New("Data Source=.;Database=Taoxue.Order;User Id=sa;Password=790825");
            ////var id = db.InsertIfNotExists<School>(school, s => s.SchoolName == school.SchoolName);

            ////var school = db.Query<School>().Where(s => s.Id == 7).FirstOrDefault();
            //var school = db.Load<School>(6);
            //if(school == null)
            //{
            //    Console.WriteLine("机构不存在");
            //}
            //else
            //{
            //    school.SchoolName += "修改过的";
            //    var row = db.Update<School>(school);
            //    Console.WriteLine("修改结果：" + row);
            //    if (row > 0)
            //    {
            //        Console.WriteLine("修改成功");
            //    }
            //    else
            //    {
            //        Console.WriteLine("修改失败");
            //    }
            //}

            #endregion

            //var resolver = new SelectConditionResolver<Student>();
            //var schoolId = 4;
            //var dt = DateTime.Today;
            //Expression<Func<Student, bool>> expr = s => true && Check() && s.StudentName.Contains("abc") && !s.IsDel && s.School.Id == schoolId && s.CreateAt < dt;
            //resolver.Resolve(expr.Body);

            //var result = resolver.GetResult();

            //Console.WriteLine(result.Condition);
            //var parameters = result.Parameters;
            //foreach (var parameter in parameters.Parameters)
            //{
            //    Console.WriteLine($"{parameter.ParameterName} = {parameter.Value}");
            //}

            //foreach (var property in result.NavPropertyList)
            //{
            //    Console.WriteLine(property);
            //}
            var db = new MyQueryable<Student>("Data Source=.;Database=Taoxue.Order;User Id=sa;Password=790825");
            var student = db.Where(s => s.Birthday == null).ToList();

            Console.WriteLine(JsonConvert.SerializeObject(student));

            //Show(2017, 5);

            Console.ReadLine();
        }

        static void Show(int year, int month)
        {
            var dt = DateTime.Parse($"{year}-{month}-1");
            for (var start = dt; dt < DateTime.Today; dt = dt.AddMonths(1))
            {
                Console.WriteLine($"{dt.Year}-{dt.Month}");
            }
        }

        static bool Check()
        {
            return false;
        }
    }
}
