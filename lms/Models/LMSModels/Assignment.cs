using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Assignment
    {
        public Assignment()
        {
            Submission = new HashSet<Submission>();
        }

        public int AssignId { get; set; }
        public string Name { get; set; }
        public int? MaxPoints { get; set; }
        public string Contents { get; set; }
        public int ClassId { get; set; }
        public int AssignType { get; set; }
        public bool? SubmitFormat { get; set; }
        public DateTime? Due { get; set; }
        public AssignmentCategory AssignTypeNavigation { get; set; }
        public Class Class { get; set; }
        public ICollection<Submission> Submission { get; set; }
    }
}
