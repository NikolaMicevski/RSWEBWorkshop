using System.ComponentModel.DataAnnotations;

namespace WorkshopV2.Models
{
    public class Teacher
    {
        public int Id { get; set; }
        [StringLength(50)]
        public string FirstName { get; set; }
        [StringLength(50)]
        public string LastName { get; set; }
        [StringLength(50)]
        public string? Degree { get; set; }
        [StringLength(25)]
        public string? AcademicRank { get; set; }
        [StringLength(10)]
        public string? OfficeNumber { get; set; }
        [DataType(DataType.Date)]
        public DateTime HireDate { get; set; }
        public string? ImageUrl { get; set; }
        public List<Course>? CoursesAsFirstTeacher { get; set; }
        public List<Course>? CoursesAsSecondTeacher { get; set; }

    }
}
