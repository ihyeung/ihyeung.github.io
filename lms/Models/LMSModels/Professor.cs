using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Professor
    {
        public Professor()
        {
            Class = new HashSet<Class>();
        }

        public Professor(string uid, string first, string last, DateTime DOB, string SubjectAbbrev)
        {
            UId = uid;
            FirstName = first;
            LastName = last;
            birth_date = DOB;
            DeptId = SubjectAbbrev;
            //TO DO: handle user password
        }

        public string UId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserPass { get; set; }
        public DateTime birth_date { get; set; }

        public string DeptId { get; set; }

        public Department Dept { get; set; }
        public ICollection<Class> Class { get; set; }
    }
}
