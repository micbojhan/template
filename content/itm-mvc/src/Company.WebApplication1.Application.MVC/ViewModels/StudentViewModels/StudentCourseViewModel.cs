using System.ComponentModel.DataAnnotations;

namespace Company.WebApplication1.Application.MVC.ViewModels.StudentViewModels
{
    public class CourseViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Titel")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "ECTS point")]
        public int Credits { get; set; }
    }
}
