using AutoMapper;
using AutoMapper.QueryableExtensions;
using Company.WebApplication1.Application.MVC.Filters;
using Company.WebApplication1.Application.MVC.ViewModels.StudentViewModels;
using Company.WebApplication1.Core.Command;
using Company.WebApplication1.Core.Entities;
using Company.WebApplication1.Core.Query;
using Company.WebApplication1.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Company.WebApplication1.Application.MVC.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public StudentController(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public IActionResult Index(int page = 1, int pageSize = 3)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 1;
            if (pageSize > 20) pageSize = 20;

            var studentSet = _dbContext.Students;

            var studentQuery = studentSet
                                .Include(x => x.Enrollments)                // Eager load enrollments and courses
                                    .ThenInclude(y => y.Course)             // because EF Core doesn't support lazy loading
                                .OrderBy(item => item.LastName)
                                .Paged(page, pageSize);

            var students = studentQuery.ProjectTo<StudentViewModel>();      // Map query to viewmodel using ProjecTo, to avoid
                                                                            // getting unneeded data from database
            var totalEntries = _dbContext.Students.Count();
            ViewBag.pageCount = Math.Ceiling(((double)totalEntries / pageSize));
            ViewBag.page = page;
            ViewBag.pageSize = pageSize;

            return View(students);
        }

        [PopulateCourses]
        public IActionResult Create()
        {
            var vm = new CreateStudentViewModel();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PopulateCourses]
        public IActionResult Create(CreateStudentViewModel createStudentViewModel, [FromServices] AddStudentCommand addStudentCommand)
        {
            // If the supplied viewmodel isn't valid, send it back
            if (!ModelState.IsValid) return View(createStudentViewModel);

            var student = _mapper.Map<Student>(createStudentViewModel);

            addStudentCommand.Student = student;
            addStudentCommand.SelectedCoursesId = createStudentViewModel.SelectedCourseIds;
            addStudentCommand.Run();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
