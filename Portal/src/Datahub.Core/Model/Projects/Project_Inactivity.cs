using System;

namespace Datahub.Core.Model.Projects
{
    public class Project_Inactivity
    {
        public int ProjectId { get; set; }
        public int DaysSinceLastLogin { get; set; }
        public bool Whitelisted { get; set; }
        public DateTime? RetirementDate { get; set; }
        public int? ThresholdNotified { get; set; }
        public DateTime? DateLastNotified { get; set; }
    }
}