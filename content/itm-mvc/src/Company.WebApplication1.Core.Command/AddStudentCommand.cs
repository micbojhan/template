using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Company.WebApplication1.Core.Entities;
using Company.WebApplication1.Infrastructure.DataAccess;

namespace Company.WebApplication1.Core.Command
{
    public class AddStudentCommand : BaseCommand<ApplicationUser>
    {
        private readonly ApplicationDbContext _dbContext;
        public Student Student;

        public IEnumerable<int> SelectedCoursesId { get; set; }

        public AddStudentCommand(ILogger<ApplicationUser> logger, ApplicationDbContext dbContext) : base(logger)
        {
            _dbContext = dbContext;
        }
        protected override void RunCommand()
        {
            // Instantiate a collection for the student object
            Student.Enrollments = new List<Enrollment>();

            // Set enrollment time to now
            Student.EnrollmentDate = new DateTime();

            // Grab all the selected courses from the database
            var selectedCourses = _dbContext.Courses.Where(x => SelectedCoursesId.Contains(x.Id));

            // Iterate through each course and add an enrollment to the student for each of them
            foreach (var course in selectedCourses)
            {
                var enrollment = new Enrollment
                {
                    Course = course,
                    Student = Student
                };
                Student.Enrollments.Add(enrollment);
            }

            _dbContext.Students.Add(Student);
            _dbContext.SaveChanges();
        }
    }
}
