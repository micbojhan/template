using System.Collections.Generic;

namespace Company.WebApplication1.Application.MVC.Filters
{
    public interface ISelectedCourses
    {
        IList<int> SelectedCourseIds { get; set; }
    }
}
