namespace SimuCanvas.Models
{
    public class Course
    {
        public int CourseId { get; set; }
        public int FacultyId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Credits { get; set; }
        public DateTime InitialDate { get; set; }
        public DateTime FinalDate { get; set; }
        public int MaxStudents { get; set; }
    }
}