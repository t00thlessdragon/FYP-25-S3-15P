using System;
using System.Collections.Generic;

namespace FYP_25_S3_15P.ViewModels
{
    public class MeetingRowVm
    {
        public string   MeetingId { get; set; } = "";
        public string   Title     { get; set; } = "";
        public DateTime Date      { get; set; }           // e.g. 2025-07-08
        public string   TimeText  { get; set; } = "";     // e.g. "2:00PM"  (use THIS name)
        public string   Organizer { get; set; } = "";
    }

    public class StudentMeetingsVm
    {
        public List<MeetingRowVm> Meetings { get; set; } = new();
    }
}
