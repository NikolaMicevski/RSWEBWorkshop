using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkshopV2.Models
{
    public class WorkshopV2User : IdentityUser
    {
        public int? TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public Teacher Teacher { get; set; }

        public long? StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student? Student { get; set; }
    }
}
