using AutoMapper;
#if (Examples)
using Company.WebApplication1.Application.MVC.ViewModels.EnrollmentViewModels;
using Company.WebApplication1.Application.MVC.ViewModels.StudentViewModels;
using Company.WebApplication1.Core.Entities;
#endif

namespace Company.WebApplication1.Application.MVC.Services
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
#if (Examples)
            CreateMap<Enrollment, EnrollmentViewModel>().ReverseMap();

            CreateMap<Student, StudentViewModel>().ReverseMap();
            CreateMap<Student, CreateStudentViewModel>().ReverseMap();
            CreateMap<Enrollment, StudentEnrollmentViewModel>()
                .ForMember(dest => dest.Grade, opt => opt.MapFrom(src => (GradeViewModel)src.Grade))
                .ReverseMap()
                .ForMember(dest => dest.Grade, opt => opt.MapFrom(src => (Grade?)src.Grade));
            CreateMap<Course, CourseViewModel>().ReverseMap();
#endif
        }
    }
}
