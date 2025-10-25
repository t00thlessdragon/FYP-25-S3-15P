namespace FYP_25_S3_15P.Models;
public class UserGroup
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int GroupId { get; set; }
    public int RoleId { get; set; }

    public User User { get; set; } = null!;
    public Group Group { get; set; } = null!;
}
