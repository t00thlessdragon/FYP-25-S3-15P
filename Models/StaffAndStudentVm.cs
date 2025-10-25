using System;
using System.Collections.Generic;

namespace FYP_25_S3_15P.Models // The namespace has been changed from .ViewModels to .Models
{
    // This model combines Staff and Student data for the UA UserMaster view.
    public class StaffAndStudentVm // Renamed from StaffAndStudentMasterVm
    {
        public List<StaffRow> Staff { get; set; } = new List<StaffRow>();
        public List<StudentRow> Students { get; set; } = new List<StudentRow>();
        
        // Nested class to handle assigned/enrolled modules
        public class ModuleRow
        {
            public int ID { get; set; }
            public int ModuleID { get; set; }
            public string ModuleCode { get; set; }
            public string ModuleName { get; set; }
        }

        // Row model for University Staff (derived from UniStaffVm)
        public class StaffRow
        {
            public int Id { get; set; } // StaffProfile ID
            public string StaffID { get; set; }
            public int UserID { get; set; } // User.Id
            public string Name { get; set; }
            public string Email { get; set; }
            public string UniAbbrv { get; set; }
            public string Status { get; set; }
            public bool IsLocked { get; set; }
            public DateTime? LastLogin { get; set; }
            public List<ModuleRow> AssignedModules { get; set; } = new List<ModuleRow>();
            public string SessionNo { get; set; }
        }

        // Row model for Students
        public class StudentRow
        {
            public int Id { get; set; } // StudentProfile ID
            public string StudentID { get; set; }
            public int UserID { get; set; } // User.Id
            public string Name { get; set; }
            public string Email { get; set; }
            public string UniAbbrv { get; set; }
            public string ProgramName { get; set; } // Specific to Students
            public string CourseName { get; set; }  // Specific to Students
            public string Status { get; set; }
            public bool IsLocked { get; set; }
            public DateTime? LastLogin { get; set; }
            public List<ModuleRow> EnrolledModules { get; set; } = new List<ModuleRow>();
            public string SessionNo { get; set; }
        }
    }
}
