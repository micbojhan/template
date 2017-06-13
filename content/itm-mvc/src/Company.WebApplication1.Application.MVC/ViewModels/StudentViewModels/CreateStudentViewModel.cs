using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Company.WebApplication1.Application.MVC.Filters;
using Company.WebApplication1.Validators;

namespace Company.WebApplication1.Application.MVC.ViewModels.StudentViewModels
{
    public class CreateStudentViewModel : ISelectedCourses
    {
        [Required]
        [RegularExpression(@"[a-zA-Zæåø ]*", ErrorMessage = "Kun bogstaver, tak")]
        [RudeName(ErrorMessage = "Navnet må ikke være ubehøvlet")]
        [Display(Name = "Efternavn")]
        public string LastName { get; set; }

        [Required]
        [RegularExpression(@"[a-zA-Zæåø ]*", ErrorMessage = "Kun bogstaver, tak")]
        [RudeName(ErrorMessage = "Navnet må ikke være ubehøvlet")]
        [Display(Name = "Fornavn og mellemnavn")]
        public string FirstMidName { get; set; }

        [Display(Name = "Indskrivelsesdato")]
        public DateTime EnrollmentDate { get; set; }

        [Display(Name = "Fag")]
        public IList<int> SelectedCourseIds { get; set; } = new List<int>{1,2,3};
    }
}
