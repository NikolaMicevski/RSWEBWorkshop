using System.ComponentModel.DataAnnotations;

namespace WorkshopV2.Models
{
    public class Student
    {
        public long Id { get; set; }
        [StringLength(10)]
        public string StudentId { get; set; }
        [StringLength(50)]
        public string FirstName { get; set; }
        [StringLength(50)]
        public string LastName { get; set; }
        [DataType(DataType.Date)]
        public DateTime? EnrollmentDate { get; set; }
        public int? AcquiredCredits { get; set; }
        public int? CurrentSemestar { get; set; }
        [StringLength(25)]
        public string? EducationLevel { get; set; }
        public string? ImageUrl { get; set; }
        public List<Enrollment>? Enrollment { get; set; }
    }
}
