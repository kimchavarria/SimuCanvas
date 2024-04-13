namespace SimuCanvas.Models
{
    public class Attendance
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public bool IsPresent { get; set; }
        public int CourseId { get; set; }
    }
}

