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
using System.Text.Json;
using System.Net;

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

        //--------search for courses--------

        [HttpGet("get/{courseId}")]
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
        public IActionResult GetCoursesList([FromBody] CourseSearch searchParameters)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(400, "Invalid input data.");
            }
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
                    return StatusCode(520, "Unknown error");
                }
            }
        }

        //--------creating courses--------

        [HttpPost("CreateCourse")]
        public IActionResult CreateCourse([FromBody] CourseShortModel course)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(400, "Invalid input data.");
            }
            try
            {
                var result = _coursesService.CreateCourse(course);
                return new JsonResult(result);
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    return StatusCode(500, "Credential Not found");
                }
                else if (e is GoogleApiException)
                {
                    return StatusCode(400, "OwnerId not specified." + e.Message);
                }
                else
                {
                    return StatusCode(520, "Unknown error");
                }
            }
        }

        [HttpPost("CreateCourseList")]
        public IActionResult CreateCourseList([FromBody] List<CourseShortModel> courses)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(400, "Invalid input data.");
            }
            try
            {

            }
            catch
            {

            }
            return null;
        }

        //--------updating courses--------

        [HttpPatch("archive/{id}")]
        public IActionResult ArchiveCourse(string id)
        {
            try
            {
                CoursePatching course = new CoursePatching();
                course.CourseState = "ARCHIVED";
                var response = _coursesService.PatchCourse(id, course);
                return new JsonResult(response);
            }
            catch (GoogleApiException e)
            {
                var errorResponse = e.HttpStatusCode;

                if (errorResponse == HttpStatusCode.NotFound)
                {
                    return StatusCode(404, "Course was not found.");
                }
                else if (errorResponse == HttpStatusCode.BadRequest)
                {
                    return StatusCode(400, "Unable to change course," +
                        " you should check that you are trying to change only the available fields" + e.Message);
                }

                return StatusCode(520, "Unknown error");
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    return StatusCode(500, "Credential Not found");
                }
                else
                {
                    return StatusCode(520, "Unknown error");
                }
            }
        }

        [HttpPatch("patch/{id}")]
        public IActionResult PatchCourse(string id, [FromBody] CoursePatching course)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(400, "Invalid input data.");
            }
            try
            {
                var response = _coursesService.PatchCourse(id, course);
                return new JsonResult(response);
            }
            catch (GoogleApiException e)
            {
                var errorResponse = e.HttpStatusCode;

                if (errorResponse == HttpStatusCode.NotFound)
                {
                    return StatusCode(404, "Course was not found.");
                }
                else if (errorResponse == HttpStatusCode.BadRequest)
                {
                    return StatusCode(400, "Unable to change course," +
                        " you should check that you are trying to change only the available fields" + e.Message);
                }

                return StatusCode(520, "Unknown error");
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    return StatusCode(500, "Credential Not found");
                }
                else
                {
                    return StatusCode(520, "Unknown error");
                }
            }
        }

        [HttpPut("update/{id}")]
        public IActionResult UpdateCourse(string id, [FromBody] CoursePatching course)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(400, "Invalid input data.");
            }
            try
            {
                var response = _coursesService.UpdateCourse(id, course);
                return new JsonResult(response);
            }
            catch (GoogleApiException e)
            {
                var errorResponse = e.HttpStatusCode;

                if (errorResponse == HttpStatusCode.NotFound)
                {
                    return StatusCode(404, "Course was not found.");
                }
                else if (errorResponse == HttpStatusCode.BadRequest)
                {
                    return StatusCode(400, "You are not permitted to modify this course or course is not modifable.");
                }

                return StatusCode(520, "Unknown error");
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    return StatusCode(500, "Credential Not found");
                }
                else
                {
                    return StatusCode(520, "Unknown error");
                }
            }
        }

        //--------deleting courses--------

        [HttpDelete("delete/{id}")]
        public IActionResult DeleteCourse(string id)
        {
            try
            {
                var result = _coursesService.DeleteCourse(id);
                return Ok("Course was deleted successfully");
            }
            catch (GoogleApiException e)
            {
                var errorResponse = e.HttpStatusCode;

                if (errorResponse == HttpStatusCode.NotFound)
                {
                    return StatusCode(404, "Course was not found.");
                }
                else if (errorResponse == HttpStatusCode.BadRequest)
                {
                    return StatusCode(400, "Precondition check failed. Perhaps you should archive the course first.");
                }

                return StatusCode(520, "Unknown error");
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    return StatusCode(500, "Credential Not found");
                }
                else
                {
                    return StatusCode(520, "Unknown error");
                }
            }
        }
    }
}
