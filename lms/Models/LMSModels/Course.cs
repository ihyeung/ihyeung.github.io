using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Course
    {
        public Course()
        {
            Class = new HashSet<Class>();
        }

        public int CourseId { get; set; }
        public string Name { get; set; }
        public int? CourseNum { get; set; }
        public string DeptId { get; set; }

        public int numCredits { get; set; }

        public Department Dept { get; set; }
        public ICollection<Class> Class { get; set; }
    }
}
