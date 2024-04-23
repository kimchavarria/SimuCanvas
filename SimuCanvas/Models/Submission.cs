namespace SimuCanvas.Models
{
    public class Submission
    {
        public int SubmissionId { get; set; }
        public int AssignmentId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string FilePath { get; set; }
        public int? Grade { get; set; }
    }
}
