using System.ComponentModel.DataAnnotations;
using Company.WebApplication1.Application.MVC.ViewModels.StudentViewModels;

namespace Company.WebApplication1.Application.MVC.ViewModels.EnrollmentViewModels
{
    public class EnrollmentViewModel
    {
        [Display(Name = "Karakter")]
        public GradeViewModel? Grade { get; set; }

        [Required]
        [Display(Name = "Fag")]
        public CourseViewModel Course { get; set; }

        [Required]
        [Display(Name = "Studerende")]
        public StudentViewModel Student { get; set; }
    }
}
