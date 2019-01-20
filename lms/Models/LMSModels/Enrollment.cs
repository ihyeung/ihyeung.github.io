using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Enrollment
    {
        public string StudentId { get; set; }
        public int ClassId { get; set; }
        public string Grade { get; set; }
        public Class Class { get; set; }
        public Student Student { get; set; }

        public Enrollment() { }
        public Enrollment(string studentId, int classId, string grade)
        {
            StudentId = studentId;
            ClassId = classId;
            Grade = grade;
        }
    }
}
