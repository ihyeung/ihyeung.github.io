using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class AssignmentCategory
    {
        public AssignmentCategory()
        {
            Assignment = new HashSet<Assignment>();
        }

        public int CatId { get; set; }
        public string Name { get; set; }

        public int Weight { get; set; }

        public int ClassId { get; set;}

        public Class Class { get; set; }

        public ICollection<Assignment> Assignment { get; set; }
    }
}
