using Google;
using HITs_classroom.Services;
using Microsoft.AspNetCore.Mvc;
using HITs_classroom.Models.Course;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using HITs_classroom.Jobs;

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

        /// <summary>
        /// Search for a course by id.
        /// </summary>
        /// <remarks>
        /// courseId - Classroom-assigned identifier or an alias (if exists).
        /// </remarks>
        /// <response code="401">Not authorized.</response>
        /// <response code="404">Course does not exist.</response>
        /// <response code="500">Credential Not found.</response>
        //[Authorize]
        [HttpGet("get/{courseId}")]
        public async Task<IActionResult> GetCourse(string courseId)
        {
            try 
            {
                var course = await _coursesService.GetCourseFromDb(courseId);
                return new JsonResult(course);
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request 'get/{{courseId}}'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
                }
                else if (e is NullReferenceException)
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
        /// Returns a list of courses from Google Classroom corresponding to the specified parameters.
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
        /// <response code="401">Not authorized.</response>
        /// <response code="404">No courses found.</response>
        /// <response code="500">Credential Not found.</response>
        //[Authorize]
        [HttpGet("listFromGC")]
        public async Task<IActionResult> GetCoursesListFromGoogleClassroom(
            [FromQuery] string? studentId,
            [FromQuery] string? teacherId,
            [FromQuery] string? courseState
            )
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
                var response = await _coursesService.GetCoursesListFromGoogleClassroom(searchParameters);
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
        /// Returns a list of courses from database corresponding to the specified parameters.
        /// </summary>
        /// <remarks>
        /// Query parameters:
        /// 
        /// courseState - restricts returned courses to those in one of the specified states.
        /// The default value is ACTIVE, ARCHIVED, PROVISIONED, DECLINED.
        /// </remarks>
        /// <response code="400">Invalid input data.</response>
        /// <response code="401">Not authorized.</response>
        /// <response code="404">No courses found.</response>
        /// <response code="500">Credential Not found.</response>
        //[Authorize]
        [HttpGet("listFromDb")]
        public async Task<IActionResult> GetCoursesListFromDb([FromQuery] string? courseState)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(400, "Invalid input data.");
            }
            try
            {
                var response = await _coursesService.GetCoursesListFromDb(courseState);
                return new JsonResult(response);
            }
            catch (Exception e)
            {
                if (e is ArgumentNullException)
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
        /// <response code="401">Not authorized.</response>
        /// <response code="404">No courses found.</response>
        /// <response code="500">Credential Not found.</response>
        //[Authorize]
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveCoursesList()
        {
            try 
            {
                var response = await _coursesService.GetActiveCoursesListFromDb();
                return new JsonResult(response);
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request 'active'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
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
        /// <response code="401">Not authorized.</response>
        /// <response code="404">No courses found.</response>
        /// <response code="500">Credential Not found.</response>
        //[Authorize]
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
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'archived'. {error}", e.Message);
                    return StatusCode(520, "Unknown error.");
                }
            }
        }

        /// <summary>
        /// Create a new course.
        /// </summary>
        /// <remarks>
        /// You can set name, ownerId, courseState, description, descriptionHeading, room, section.
        /// Returns the created course.
        /// </remarks>
        /// <response code="400">Invalid input data.</response>
        /// <response code="401">Not authorized.</response>
        /// <response code="404">OwnerId not specified.</response>
        /// <response code="500">Credential Not found.</response>
        //[Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateCourse([FromBody] CourseShortModel course)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(400, "Invalid input data.");
            }
            try
            {
                var result = await _coursesService.CreateCourse(course);
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

        /// <summary>
        /// Archive a course.
        /// </summary>
        /// <remarks>
        /// Sets the courseState to ARCHIVED.
        /// </remarks>
        /// <response code="400">Unable to change course,
        /// you should check that you are trying to change only the available fields.
        /// </response>
        /// <response code="401">Not authorized.</response>
        /// <response code="404">Course was not found.</response>
        /// <response code="500">Credential Not found.</response>
        //[Authorize]
        [HttpPatch("archive/{courseId}")]
        public async Task<IActionResult> ArchiveCourse(string courseId)
        {
            try
            {
                CoursePatching course = new CoursePatching();
                course.CourseState = "ARCHIVED";
                var response = await _coursesService.PatchCourse(courseId, course);
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
                else if (e is NullReferenceException)
                {
                    _logger.LogInformation("An error was found when executing the request 'archive{{courseId}}'. {error}", e.Message);
                    return StatusCode(404, "Course not found in database.");
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
        /// <response code="401">Not authorized.</response>
        /// <response code="404">Course was not found.</response>
        /// <response code="500">Credential Not found.</response>
        //[Authorize]
        [HttpPatch("patch/{courseId}")]
        public async Task<IActionResult> PatchCourse(string courseId, [FromBody] CoursePatching course)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(400, "Invalid input data.");
            }
            try
            {
                var response = await _coursesService.PatchCourse(courseId, course);
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
                else if (e is NullReferenceException)
                {
                    _logger.LogInformation("An error was found when executing the request 'patch{{courseId}}'. {error}", e.Message);
                    return StatusCode(404, "Course not found in database.");
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
        /// <response code="401">Not authorized.</response>
        /// <response code="404">Course was not found.</response>
        /// <response code="500">Credential Not found.</response>
        //[Authorize]
        [HttpPut("update/{courseId}")]
        public async Task<IActionResult> UpdateCourse(string courseId, [FromBody] CoursePatching course)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(400, "Invalid input data.");
            }
            try
            {
                var response = await _coursesService.UpdateCourse(courseId, course);
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
                else if (e is NullReferenceException)
                {
                    _logger.LogInformation("An error was found when executing the request 'update{{courseId}}'. {error}", e.Message);
                    return StatusCode(404, "Course not found in database.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'update{{courseId}}'. {error}", e.Message);
                    return StatusCode(520, "Unknown error.");
                }
            }
        }

        /// <summary>
        /// Delete a course.
        /// </summary>
        /// <remarks>
        /// Deletes a course by id. You should archive course before deleting it.
        /// </remarks>
        /// <response code="400">Precondition check failed. Perhaps you should archive the course first.</response>
        /// <response code="401">Not authorized.</response>
        /// <response code="404">Course was not found.</response>
        /// <response code="500">Credential Not found.</response>
        //[Authorize]
        [HttpDelete("delete/{courseId}")]
        public async Task<IActionResult> DeleteCourse(string courseId)
        {
            try
            {
                await _coursesService.DeleteCourse(courseId);
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
                else if (e is NullReferenceException)
                {
                    _logger.LogInformation("An error was found when executing the request 'delete{{courseId}}'. {error}", e.Message);
                    return StatusCode(404, "Course not found in database.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'delete{{courseId}}'. {error}", e.Message);
                    return StatusCode(520, "Unknown error.");
                }
            }
        }

        /// <summary>
        /// Synchronizes courses.
        /// </summary>
        /// <remarks>
        /// Synchronizes courses in the database and in the Google classroom 
        /// (adds courses that are in the Google classroom, but not in the database).
        /// </remarks>
        /// <response code="400">Google api exception.</response>
        /// <response code="401">Not authorized.</response>
        /// <response code="500">Credential Not found.</response>
        //[Authorize]
        [HttpPost("synchronize")]
        public async Task<IActionResult> SynchronizeCourses()
        {
            try
            {
                var response = await _coursesService.SynchronizeCoursesListsInDbAndGoogleClassroom();
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
                    return StatusCode(400, "GoogleApi exception.");
                }
                else if (e is NullReferenceException)
                {
                    _logger.LogInformation("An error was found when executing the request 'synchronize'. {error}", e.Message);
                    return StatusCode(404, "Course not found in database.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'synchronize'. {error}", e.Message);
                    return StatusCode(520, "Unknown error.");
                }
            }
        }

        //[Authorize]
        [HttpPost("createList")]
        public IActionResult CreateCoursesList([FromBody] List<CourseShortModel> courses)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(400, "Invalid input data.");
            }
            Claim? relatedUser = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            if (relatedUser == null)
            {
                _logger.LogInformation("An error was found when executing the request 'createList'. {error}", "Email not found.");
                return StatusCode(401, "Not authorized.");
            }
            CoursesScheduler.Start(relatedUser.Value, courses);
            return Ok();
        }
    }
}
