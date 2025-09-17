using System;
using System.Collections.Generic;

namespace FYP_25_S3_15P.ViewModels
{
    public class AssessorTaskRowVm
    {
        public string   TaskId       { get; set; } = "";
        public string   Title        { get; set; } = "";
        public DateTime DueDate      { get; set; }
        public DateTime SubmittedOn  { get; set; }
        public string   Status       { get; set; } = "";   // e.g., Submitted / Pending
    }

    public class AssessorTaskDeliverablesVm
    {
        // Header
        public string ProjectId { get; set; } = "";
        public string Title     { get; set; } = "";
        public string GroupId   { get; set; } = "";
        public string Supervisor{ get; set; } = "";
        public string Assessor  { get; set; } = "";
        public List<string> Members { get; set; } = new();

        // Rows
        public List<AssessorTaskRowVm> Tasks { get; set; } = new();
    }
}
