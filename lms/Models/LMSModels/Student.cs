using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Student
    {
        public Student()
        {
            Enrollment = new HashSet<Enrollment>();
            Submission = new HashSet<Submission>();
        }

        public string UId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserPass { get; set; }
        public DateTime birth_date { get; set; }
        public string DeptId { get; set; }

        public Department Dept { get; set; }
        public ICollection<Enrollment> Enrollment { get; set; }
        public ICollection<Submission> Submission { get; set; }

        public Student(string uid, string first, string last, DateTime DOB, string SubjectAbbrev)
        {
            UId = uid;
            FirstName = first;
            LastName = last;
            birth_date = DOB;
            DeptId = SubjectAbbrev;

        }
    }
}
