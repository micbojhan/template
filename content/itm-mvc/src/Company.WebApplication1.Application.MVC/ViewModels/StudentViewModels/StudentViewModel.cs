using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Company.WebApplication1.Application.MVC.ViewModels.StudentViewModels
{
    public class StudentViewModel
    {
        [Display(Name = "Efternavn")]
        public string LastName { get; set; }

        [Display(Name = "Fornavn og mellemnavn")]
        public string FirstMidName { get; set; }

        [Display(Name = "Indskrivelsesdato")]
        public DateTime EnrollmentDate { get; set; }

        [Display(Name = "Fag")]
        public IList<StudentEnrollmentViewModel> Enrollments { get; set; }
    }
}
