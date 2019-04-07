using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1
{
    public class Student2
    {
        public int StudentId { get; set; }

        public string StudentName { get; set; }

        public string Mobile { get; set; }

        public string Card { get; set; }

        public string State { get; set; }

        public DateTime? Birthday { get; set; }

        public int FKSchoolId { get; set; }

        public string Owner { get; set; }

        public bool IsDel { get; set; }

        public DateTime CreateAt { get; set; }

        public string CreateBy { get; set; }

        public DateTime UpdateAt { get; set; }

        public string UpdateBy { get; set; }

        public School School { get; set; }
    }
}
