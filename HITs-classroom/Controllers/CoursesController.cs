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
using System.Xml.Linq;

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

        /// <summary>
        /// Search for a course by id.
        /// </summary>
        /// <remarks>
        /// courseId - Classroom-assigned identifier or an alias (if exists).
        /// </remarks>
        /// <response code="404">Course does not exist.</response>
        /// <response code="500">Credential Not found.</response>
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
                    return StatusCode(500, "Credential Not found.");
                }
                else if (e is GoogleApiException)
                {
                    return StatusCode(404, "Course does not exist.");
                }
                else
                {
                    return StatusCode(520, "Unknown error.");
                }
            }
        }

        /// <summary>
        /// Returns a list of courses corresponding to the specified parameters.
        /// </summary>
        /// <remarks>
        /// studentId - restricts returned courses to those having a student with the specified identifier.
        /// teacherId - restricts returned courses to those having a teacher with the specified identifier.
        /// (the numeric identifier for the user, the email address of the user, the string literal "me", indicating the requesting user).
        /// courseState - Restricts returned courses to those in one of the specified states.
        /// The default value is ACTIVE, ARCHIVED, PROVISIONED, DECLINED.
        /// </remarks>
        /// <response code="400">Invalid input data.</response>
        /// <response code="404">No courses found.</response>
        /// <response code="500">Credential Not found.</response>
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
                    return StatusCode(500, "Credential Not found.");
                }
                else if (e is ArgumentNullException)
                {
                    return StatusCode(404, "No courses found.");
                }
                else
                {
                    return StatusCode(520, "Unknown error.");
                }
            }
        }

        /// <summary>
        /// Returns a list of active courses.
        /// </summary>
        /// <remarks>
        /// Returns courses with the ACTIVE courseState.
        /// </remarks>
        /// <response code="404">No courses found.</response>
        /// <response code="500">Credential Not found.</response>
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
                    return StatusCode(500, "Credential Not found.");
                }
                else if (e is GoogleApiException)
                {
                    return StatusCode(404, "No courses found.");
                }
                else
                {
                    return StatusCode(520, "Unknown error.");
                }
            }
        }

        /// <summary>
        /// Returns a list of achived courses.
        /// </summary>
        /// <remarks>
        /// Returns courses with the ACHIVED courseState.
        /// </remarks>
        /// <response code="404">No courses found.</response>
        /// <response code="500">Credential Not found.</response>
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
                    return StatusCode(500, "Credential Not found.");
                }
                else if (e is GoogleApiException)
                {
                    return StatusCode(404, "No courses found.");
                }
                else
                {
                    return StatusCode(520, "Unknown error.");
                }
            }
        }

        //--------creating courses--------

        /// <summary>
        /// Create a new course.
        /// </summary>
        /// <remarks>
        /// You can set name, ownerId, courseState, description, descriptionHeading, room, section.
        /// Returns the created course.
        /// </remarks>
        /// <response code="400">Invalid input data.</response>
        /// <response code="404">OwnerId not specified.</response>
        /// <response code="500">Credential Not found.</response>
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
                    return StatusCode(500, "Credential Not found.");
                }
                else if (e is GoogleApiException)
                {
                    return StatusCode(404, "OwnerId not specified.");
                }
                else
                {
                    return StatusCode(520, "Unknown error.");
                }
            }
        }

        //[HttpPost("CreateCoursesList")]
        //public IActionResult CreateCoursesList([FromBody] List<CourseShortModel> courses)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return StatusCode(400, "Invalid input data.");
        //    }
        //    try
        //    {
        //        var response = _coursesService.CreateCoursesList(courses);
        //        return new JsonResult(response);
        //    }
        //    catch (Exception e)
        //    {
        //        if (e is AggregateException)
        //        {
        //            return StatusCode(500, "Credential Not found");
        //        }
        //        else if (e is GoogleApiException)
        //        {
        //            return StatusCode(400, "OwnerId not specified.");
        //        }
        //        else
        //        {
        //            return StatusCode(520, "Unknown error");
        //        }
        //    }
        //}

        //--------updating courses--------

        /// <summary>
        /// Archive a course.
        /// </summary>
        /// <remarks>
        /// Sets the courseState to ARCHIVED.
        /// </remarks>
        /// <response code="400">Unable to change course,
        /// you should check that you are trying to change only the available fields.
        /// </response>
        /// <response code="404">Course was not found.</response>
        /// <response code="500">Credential Not found.</response>
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
                        " you should check that you are trying to change only the available fields");
                }

                return StatusCode(520, "Unknown error");
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    return StatusCode(500, "Credential Not found.");
                }
                else
                {
                    return StatusCode(520, "Unknown error");
                }
            }
        }

        /// <summary>
        /// Patch a course.
        /// </summary>
        /// <remarks>
        /// The following fields are valid: name, section, descriptionHeading, description, room, courseState, ownerId.
        /// </remarks>
        /// <response code="400">Unable to change course,
        /// you should check that you are trying to change only the available fields.</response>
        /// <response code="404">Course was not found.</response>
        /// <response code="500">Credential Not found.</response>
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
                        " you should check that you are trying to change only the available fields.");
                }

                return StatusCode(520, "Unknown error.");
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    return StatusCode(500, "Credential Not found.");
                }
                else
                {
                    return StatusCode(520, "Unknown error.");
                }
            }
        }

        /// <summary>
        /// Update a course.
        /// </summary>
        /// <remarks>
        /// The following fields are valid: name, section, descriptionHeading, description, room, courseState, ownerId.
        /// </remarks>
        /// <response code="400">You are not permitted to modify this course or course is not modifable.</response>
        /// <response code="404">Course was not found.</response>
        /// <response code="500">Credential Not found.</response>
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

                return StatusCode(520, "Unknown error.");
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    return StatusCode(500, "Credential Not found.");
                }
                else
                {
                    return StatusCode(520, "Unknown error.");
                }
            }
        }

        //--------deleting courses--------

        /// <summary>
        /// Delete a course.
        /// </summary>
        /// <remarks>
        /// Deletes a course by id. You should archive course before deleting it.
        /// </remarks>
        /// <response code="400">Precondition check failed. Perhaps you should archive the course first.</response>
        /// <response code="404">Course was not found.</response>
        /// <response code="500">Credential Not found.</response>
        [HttpDelete("delete/{id}")]
        public IActionResult DeleteCourse(string id)
        {
            try
            {
                var result = _coursesService.DeleteCourse(id);
                return Ok("Course was deleted successfully.");
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

                return StatusCode(520, "Unknown error.");
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    return StatusCode(500, "Credential Not found.");
                }
                else
                {
                    return StatusCode(520, "Unknown error.");
                }
            }
        }
    }
}
