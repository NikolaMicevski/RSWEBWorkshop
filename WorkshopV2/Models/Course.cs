using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WorkshopV2.Models
{
    public class Course
    {
        public int Id { get; set; }
        [StringLength(100)]
        public string Title { get; set; }
        public int Credits { get; set; }
        public int Semester { get; set; }
        [StringLength(100)]
        public string? Programme { get; set; }
        [StringLength(25)]
        public string? EducationLevel { get; set; }
        public int? FirstTeacherId { get; set; }
        public int? SecondTeacherId { get; set; }
        public List<Enrollment>? Enrollment { get; set; }
        [ForeignKey("FirstTeacherId")]
        public Teacher? FirstTeacher { get; set; }

        [ForeignKey("SecondTeacherId")]
        public Teacher? SecondTeacher { get; set; }

    }
}
