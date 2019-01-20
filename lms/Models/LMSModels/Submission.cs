using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Submission
    {
        public int SubId { get; set; }
        public string UserId { get; set; }
        public int AssignId { get; set; }
        public DateTimeOffset SubmitTime { get; set; }
        public double? NumPoints { get; set; }
        public string TextSub { get; set; }
        public byte[] BinSub { get; set; }

        public Assignment Assign { get; set; }
        public Student User { get; set; }
    }
}
