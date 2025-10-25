using System.ComponentModel.DataAnnotations;

namespace FYP_25_S3_15P.Models
{
    public class FAQ
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Question { get; set; } = "";

        [Required] public string Answer { get; set; } = "";

        [Range(0, 9999)] public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedUtc { get; set; }
    }
}