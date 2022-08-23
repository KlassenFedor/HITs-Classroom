using Google.Apis.Auth.OAuth2;
using Google.Apis.Classroom.v1.Data;
using Google.Apis.Classroom.v1;
using Google.Apis.Services;
using Google;
using HITs_classroom.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Google.Apis.Util.Store;
using HITs_classroom.Models.Course;

namespace HITs_classroom.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly ICoursesService _coursesService;
        public CoursesController(ICoursesService coursesService)
        {
            _coursesService = coursesService;
        }

        [HttpGet("{courseId}")]
        public IActionResult GetCourse(string courseId)
        {
            try 
            {
                Course course = _coursesService.GetCourse(courseId);
                return new JsonResult(course);
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    return StatusCode(500, "Credential Not found");
                }
                else if (e is GoogleApiException)
                {
                    return StatusCode(404, "Course does not exist.");
                }
                else
                {
                    return StatusCode(520, "Unknown error");
                }
            }
        }

        [HttpPost("Courses")]
        public IActionResult GetCoursesList(CourseSearch searchParameters)
        {
            try
            {
                var response = _coursesService.GetCoursesList(searchParameters);
                return new JsonResult(response);
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    return StatusCode(500, "Credential Not found");
                }
                else if (e is GoogleApiException)
                {
                    return StatusCode(404, "Course does not exist.");
                }
                else
                {
                    return StatusCode(520, "Unknown error");
                }
            }
        }

        [HttpGet("ActiveCourses")]
        public IActionResult GetActiveCoursesList()
        {
            try 
            { 
                var response = _coursesService.GetActiveCoursesList();
                return new JsonResult(response);
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    return StatusCode(500, "Credential Not found");
                }
                else if (e is GoogleApiException)
                {
                    return StatusCode(404, "Course does not exist.");
                }
                else
                {
                    return StatusCode(520, "Unknown error");
                }
            }
        }

        [HttpGet("ArchivedCourses")]
        public IActionResult GetArchivedCoursesList()
        {
            try
            {
                var response = _coursesService.GetArchivedCoursesList();
                return new JsonResult(response);
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    return StatusCode(500, "Credential Not found");
                }
                else if (e is GoogleApiException)
                {
                    return StatusCode(404, "Course does not exist.");
                }
                else
                {
                    throw;
                    return StatusCode(520, "Unknown error");
                }
            }
        }

        [HttpPost("CreateCourse")]
        public IActionResult CreateCourse(Course course)
        {
            return null;
        }
    }
}
