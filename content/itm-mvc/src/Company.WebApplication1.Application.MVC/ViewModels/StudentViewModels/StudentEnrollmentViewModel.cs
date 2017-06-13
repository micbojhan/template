using System.ComponentModel.DataAnnotations;

namespace Company.WebApplication1.Application.MVC.ViewModels.StudentViewModels
{
    public class StudentEnrollmentViewModel
    {
        [Display(Name = "Karakter")]
        public GradeViewModel? Grade { get; set; }

        [Required]
        [Display(Name = "Fag")]
        public CourseViewModel Course { get; set; }
    }
}
