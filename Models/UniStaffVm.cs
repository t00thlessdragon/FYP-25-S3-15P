using System;
using System.Collections.Generic;

namespace FYP_25_S3_15P.Models
{
    public class UniStaffVm
    {
        public List<Row> Staff { get; set; } = new List<Row>();

        public class Row
        {
            public int Id { get; set; }
            public string StaffID { get; set; }
            public int? UserID { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string UniAbbrv { get; set; }  // University Abbreviation
            public string Status { get; set; }
            public bool IsLocked { get; set; }
            public DateTime? LastLogin { get; set; }
            public List<ModuleRow> AssignedModules { get; set; } = new List<ModuleRow>();
            public string SessionNo { get; set; }

        }
            public class ModuleRow
            {
                public int ID { get; set; }
                public int ModuleID { get; set; }
                public string ModuleCode { get; set; }
                public string ModuleName { get; set; }
            }
    }
}
