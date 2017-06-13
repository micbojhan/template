using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Company.WebApplication1.Application.MVC.ViewModels.EnrollmentViewModels;

namespace Company.WebApplication1.Application.MVC.ViewModels.CourseViewModels
{
    public class CourseViewModel
    {
        [Required]
        [Display(Name = "Titel")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "ECTS point")]
        public int Credits { get; set; }

        [Display(Name = "Tilmeldinger")]
        public IList<EnrollmentViewModel> Enrollments { get; set; }
    }
}
