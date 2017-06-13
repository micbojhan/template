using Company.WebApplication1.Infrastructure.DataAccess;
using Company.WebApplication1.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Company.WebApplication1.Application.MVC.Filters
{
    public class PopulateCoursesAttribute : TypeFilterAttribute
    {
        public PopulateCoursesAttribute() : base(typeof(PopulateCoursesFilter))
        {
        }

        private class PopulateCoursesFilter : IAsyncResultFilter
        {
            private readonly ApplicationDbContext _dbContext;

            public PopulateCoursesFilter(ApplicationDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
            {
                if (context.Result is ViewResult)
                {
                    var controller = context.Controller as Controller;
                    if (controller == null) return;

                    if (controller.ViewData.Model == null)
                    {
                        throw new NullReferenceException("View model is null, remember to provide view with an instance of the view model");
                    }

                    var viewModel = controller.ViewData.Model as ISelectedCourses;

                    if (viewModel == null)
                    {
                        throw new InvalidCastException($"View model doesn't not implement {nameof(ISelectedCourses)}");
                    }

                    var courses = await _dbContext.Courses
                                            .Where(item => !item.IsDeleted || viewModel.SelectedCourseIds.Contains(item.Id))
                                            .OrderBy(item => item.Title)
                                            .ToListAsync();

                    controller.ViewBag.AvailableCourses = new SelectList(courses, nameof(Course.Id), nameof(Course.Title));
                }
                await next();
            }
        }
    }
}
