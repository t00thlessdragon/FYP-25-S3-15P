using System;
using System.Collections.Generic;

namespace FYP_25_S3_15P.ViewModels
{
    public class ApplicationMasterVm
    {
        public IEnumerable<Row> Applications { get; set; } = Array.Empty<Row>();

        public class Row
        {
            public int AppId { get; set; }
            public string ApplicantName { get; set; } = "";
            public string Email { get; set; } = "";
            public string Role { get; set; } = "";
            public string UniName { get; set; } = "";   // <- used by view
            public string UEN { get; set; } = "";       // <- used by view
            public int PlanId { get; set; }
            public string PlanName { get; set; } = "";  // <- used by view
            public string Status { get; set; } = "Pending";
            public DateTime CreatedAt { get; set; }

            public string SubmittedAtLocal =>
                CreatedAt.ToLocalTime().ToString("d/M/yyyy h:mm tt"); // <- used by view
        }
    }
}