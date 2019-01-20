using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Administrator
    {
        public Administrator(string uid, string first, string last, DateTime DOB)
        {
            UId = uid;
            FirstName = first;
            LastName = last;
            birth_date = DOB;
            //TO DO: handle user password
        }
        public string UId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserPass { get; set; }
        public DateTime birth_date { get; set; }
    }
}
