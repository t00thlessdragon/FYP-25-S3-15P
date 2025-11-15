using System.Collections.Generic;

namespace FYP_25_S3_15P.Models
{
    public class Group
    {
        public int Id { get; set; }

        // e.g. FYP-25-S3-15P
        public string GroupName { get; set; } = string.Empty;

        public bool IsFullTime { get; set; }

        // Foreign key to FYPTopics
        public string? ProjectId { get; set; }
        public Project? Project { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }

        // Navigation to UserGroups (students, supervisors, assessors)
        public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
    }
}
