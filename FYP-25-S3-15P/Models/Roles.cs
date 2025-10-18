// Models/Role.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FYP_25_S3_15P.Models
{
    [Table("Roles", Schema = "dbo")]
    [Index(nameof(Name), IsUnique = true)] // remove if you don't want unique role names
    public class Role
    {
        [Key]
        [Column("RoleId")]
        public int ID { get; set; }          // PK -> dbo.Roles.RoleId

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(400)]
        public string? Description { get; set; }

        // Navigation: all users with this role
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}