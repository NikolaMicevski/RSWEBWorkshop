namespace WorkshopV2.Models.ViewModels
{
    public class StudentEnrollmentViewModel
    {
        public long StudentId { get; set; }
        public string FullName { get; set; }
        public bool IsEnrolled { get; set; }
        public bool IsFinished { get; set; }
    }
}
