namespace SimuCanvas.Models
{
    public class Groups
    {
        public int GroupId { get; set; }
        public int CourseId { get; set; }
        public string GroupName { get; set; }
        public List<string> Members { get; set; } //need to remove

    }
}

