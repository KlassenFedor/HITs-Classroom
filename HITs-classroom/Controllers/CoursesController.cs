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
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace HITs_classroom.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly ICoursesService _coursesService;
        private readonly ILogger _logger;
        public CoursesController(ICoursesService coursesService, ILogger<CoursesController> logger)
        {
            _coursesService = coursesService;
            _logger = logger;
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
        [Authorize]
        [HttpGet("get/{courseId}")]
        public async Task<IActionResult> GetCourse(string courseId)
        {
            try 
            {
                string? relatedUser = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
                CourseInfoModel course = await _coursesService.GetCourseFromDb(courseId, relatedUser);
                return new JsonResult(course);
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request 'get/{{courseId}}'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
                }
                else if (e is GoogleApiException)
                {
                    _logger.LogInformation("An error was found when executing the request 'get/{{courseId}}'. {error}", e.Message);
                    return StatusCode(404, "Course does not exist.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'get/{{courseId}}'. {error}", e.Message);
                    return StatusCode(520, "Unknown error.");
                }
            }
        }

        /// <summary>
        /// Returns a list of courses corresponding to the specified parameters.
        /// </summary>
        /// <remarks>
        /// Query parameters:
        /// 
        /// studentId - restricts returned courses to those having a student with the specified identifier.
        /// 
        /// teacherId - restricts returned courses to those having a teacher with the specified identifier.
        /// (the numeric identifier for the user, the email address of the user, the string literal "me", indicating the requesting user).
        /// 
        /// courseState - restricts returned courses to those in one of the specified states.
        /// The default value is ACTIVE, ARCHIVED, PROVISIONED, DECLINED.
        /// </remarks>
        /// <response code="400">Invalid input data.</response>
        /// <response code="404">No courses found.</response>
        /// <response code="500">Credential Not found.</response>
        [Authorize]
        [HttpGet("list")]
        public async Task<IActionResult> GetCoursesList([FromQuery] string? studentId, [FromQuery] string? teacherId, [FromQuery] string? courseState)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(400, "Invalid input data.");
            }
            try
            {
                var searchParameters = new CourseSearch();
                searchParameters.StudentId = studentId;
                searchParameters.TeacherId = teacherId;
                searchParameters.CourseState = courseState;
                string? relatedUser = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
                var response = await _coursesService.GetCoursesListFromGoogleClassroom(searchParameters, relatedUser);
                return new JsonResult(response);
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request 'list'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
                }
                else if (e is ArgumentNullException)
                {
                    _logger.LogInformation("An error was found when executing the request 'list'. {error}", e.Message);
                    return StatusCode(404, "No courses found.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'list'. {error}", e.Message);
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
        [HttpGet("active")]
        public IActionResult GetActiveCoursesList()
        {
            try 
            { 
                var response = _coursesService.GetActiveCoursesListFromDb();
                return new JsonResult(response);
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request 'active'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
                }
                else if (e is GoogleApiException)
                {
                    _logger.LogInformation("An error was found when executing the request 'active'. {error}", e.Message);
                    return StatusCode(404, "No courses found.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'active'. {error}", e.Message);
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
        [HttpGet("archived")]
        public IActionResult GetArchivedCoursesList()
        {
            try
            {
                var response = _coursesService.GetArchivedCoursesListFromDb();
                return new JsonResult(response);
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request 'archived'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
                }
                else if (e is GoogleApiException)
                {
                    _logger.LogInformation("An error was found when executing the request 'archived'. {error}", e.Message);
                    return StatusCode(404, "No courses found.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'archived'. {error}", e.Message);
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
        [HttpPost("create")]
        public IActionResult CreateCourse([FromBody] CourseShortModel course)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(400, "Invalid input data.");
            }
            try
            {
                string? relatedUser = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
                var result = _coursesService.CreateCourse(course, relatedUser);
                return new JsonResult(result);
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request 'create'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
                }
                else if (e is GoogleApiException)
                {
                    _logger.LogInformation("An error was found when executing the request 'create'. {error}", e.Message);
                    return StatusCode(404, "OwnerId not specified." + e.Message);
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'create'. {error}", e.Message);
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
        [HttpPatch("archive/{courseId}")]
        public IActionResult ArchiveCourse(string courseId)
        {
            try
            {
                CoursePatching course = new CoursePatching();
                course.CourseState = "ARCHIVED";
                string? relatedUser = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
                var response = _coursesService.PatchCourse(courseId, course, relatedUser);
                return new JsonResult(response);
            }
            catch (GoogleApiException e)
            {
                var errorResponse = e.HttpStatusCode;

                if (errorResponse == HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("An error was found when executing the request 'archive{{courseId}}'. {error}", e.Message);
                    return StatusCode(404, "Course was not found.");
                }
                else if (errorResponse == HttpStatusCode.BadRequest)
                {
                    _logger.LogInformation("An error was found when executing the request 'archive{{courseId}}'. {error}", e.Message);
                    return StatusCode(400, "Unable to change course," +
                        " you should check that you are trying to change only the available fields");
                }

                return StatusCode(520, "Unknown error");
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request 'archive{{courseId}}'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'archive{{courseId}}'. {error}", e.Message);
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
        [HttpPatch("patch/{courseId}")]
        public IActionResult PatchCourse(string courseId, [FromBody] CoursePatching course)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(400, "Invalid input data.");
            }
            try
            {
                string? relatedUser = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
                var response = _coursesService.PatchCourse(courseId, course, relatedUser);
                return new JsonResult(response);
            }
            catch (GoogleApiException e)
            {
                var errorResponse = e.HttpStatusCode;

                if (errorResponse == HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("An error was found when executing the request 'patch{{courseId}}'. {error}", e.Message);
                    return StatusCode(404, "Course was not found.");
                }
                else if (errorResponse == HttpStatusCode.BadRequest)
                {
                    _logger.LogInformation("An error was found when executing the request 'patch{{courseId}}'. {error}", e.Message);
                    return StatusCode(400, "Unable to change course," +
                        " you should check that you are trying to change only the available fields.");
                }

                _logger.LogInformation("An error was found when executing the request 'patch{{courseId}}'. {error}", e.Message);
                return StatusCode(520, "Unknown error.");
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request 'patch{{courseId}}'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'patch{{courseId}}'. {error}", e.Message);
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
        [HttpPut("update/{courseId}")]
        public IActionResult UpdateCourse(string courseId, [FromBody] CoursePatching course)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(400, "Invalid input data.");
            }
            try
            {
                string? relatedUser = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
                var response = _coursesService.UpdateCourse(courseId, course, relatedUser);
                return new JsonResult(response);
            }
            catch (GoogleApiException e)
            {
                var errorResponse = e.HttpStatusCode;

                if (errorResponse == HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("An error was found when executing the request 'update{{courseId}}'. {error}", e.Message);
                    return StatusCode(404, "Course was not found.");
                }
                else if (errorResponse == HttpStatusCode.BadRequest)
                {
                    _logger.LogInformation("An error was found when executing the request 'update{{courseId}}'. {error}", e.Message);
                    return StatusCode(400, "You are not permitted to modify this course or course is not modifable.");
                }

                return StatusCode(520, "Unknown error.");
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request 'update{{courseId}}'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'update{{courseId}}'. {error}", e.Message);
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
        [HttpDelete("delete/{courseId}")]
        public IActionResult DeleteCourse(string courseId)
        {
            try
            {
                string? relatedUser = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
                var result = _coursesService.DeleteCourse(courseId, relatedUser);
                return new JsonResult("Course was deleted successfully.");
            }
            catch (GoogleApiException e)
            {
                var errorResponse = e.HttpStatusCode;

                if (errorResponse == HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("An error was found when executing the request 'delete{{courseId}}'. {error}", e.Message);
                    return StatusCode(404, "Course was not found.");
                }
                else if (errorResponse == HttpStatusCode.BadRequest)
                {
                    _logger.LogInformation("An error was found when executing the request 'delete{{courseId}}'. {error}", e.Message);
                    return StatusCode(400, "Precondition check failed. Perhaps you should archive the course first.");
                }

                return StatusCode(520, "Unknown error.");
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request 'delete{{courseId}}'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'delete{{courseId}}'. {error}", e.Message);
                    return StatusCode(520, "Unknown error.");
                }
            }
        }

        [HttpPost("synchronize")]
        public async Task<IActionResult> SynchronizeCourses()
        {
            try
            {
                string? relatedUser = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
                var response = await _coursesService.SynchronizeCoursesListsInDbAndGoogleClassroom(relatedUser);
                return new JsonResult(response);
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request 'synchronize'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
                }
                else if (e is GoogleApiException)
                {
                    _logger.LogInformation("An error was found when executing the request 'synchronize'. {error}", e.Message);
                    return StatusCode(404, "GoogleApi exception.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'synchronize'. {error}", e.Message);
                    return StatusCode(520, "Unknown error.");
                }
            }
        }
    }
}
