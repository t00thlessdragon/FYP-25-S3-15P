using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace FYP_25_S3_15P.Models;
public class UserRole
{
    public int ID { get; set; }

    // Foreign Keys
    public int UserID { get; set; }
    public int RoleID { get; set; }

    // Navigation properties
    public User User { get; set; }
    public Role Role { get; set; }
}