using System.Collections.Generic;

namespace Company.WebApplication1.Core.Entities
{
    public class Course : IDeletable
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Credits { get; set; }
        public bool IsDeleted { get; set; }
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
