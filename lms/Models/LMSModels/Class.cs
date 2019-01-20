using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Class
    {
        public Class()
        {
            Assignment = new HashSet<Assignment>();
            Enrollment = new HashSet<Enrollment>();
        }

        public int ClassId { get; set; }
        public int CourseId { get; set; }
        public string Location { get; set; }
        public TimeSpan? start_time { get; set; }
        public TimeSpan? end_time { get; set; }
        public string ProfUId { get; set; }
        public Course Course { get; set; }
        public int year { get; set; }
        public string Semester { get; set; }
        public Professor ProfU { get; set; }
        public ICollection<Assignment> Assignment { get; set; }
        public ICollection<Enrollment> Enrollment { get; set; }
        public ICollection<AssignmentCategory> AssignmentCategories { get; set; }


    }
}
