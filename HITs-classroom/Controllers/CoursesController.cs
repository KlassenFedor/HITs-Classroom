using Google;
using HITs_classroom.Services;
using Microsoft.AspNetCore.Mvc;
using HITs_classroom.Models.Course;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace HITs_classroom.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CoursesController : ControllerBase
    {
        private readonly ICoursesService _coursesService;
        private readonly ILogger _logger;
        private readonly ICoursesListService _coursesListService;
        public CoursesController(
            ICoursesService coursesService,
            ILogger<CoursesController> logger,
            ICoursesListService coursesListService)
        {
            _coursesService = coursesService;
            _logger = logger;
            _coursesListService = coursesListService;
        }

        /// <summary>
        /// Search for a course by id.
        /// </summary>
        /// <remarks>
        /// courseId - Classroom-assigned identifier or an alias (if exists).
        /// </remarks>
        /// <response code="401">Not authorized.</response>
        /// <response code="404">Course does not exist.</response>
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
                if (e is NullReferenceException)
                {
                    _logger.LogError("An error was found when executing the request 'get/{{courseId}}'. {error}", e.Message);
                    return StatusCode(404, "Course does not exist.");
                }
                else
                {
                    _logger.LogError("An error was found when executing the request 'get/{{courseId}}'. {error}", e.Message);
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
                    _logger.LogError("An error was found when executing the request 'list'. {error}", e.Message);
                    return StatusCode(404, "No courses found.");
                }
                else
                {
                    _logger.LogError("An error was found when executing the request 'list'. {error}", e.Message);
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
                _logger.LogError("An error was found when executing the request 'active'. {error}", e.Message);
                return StatusCode(520, "Unknown error.");
            }
        }

        /// <summary>
        /// Returns a list of achived courses.
        /// </summary>
        /// <remarks>
        /// Returns courses with the ACHIVED courseState.
        /// </remarks>
        /// <response code="401">Not authorized.</response>
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
                _logger.LogError("An error was found when executing the request 'archived'. {error}", e.Message);
                return StatusCode(520, "Unknown error.");
            }
        }

        /// <summary>
        /// Create a new course.
        /// </summary>
        /// <remarks>
        /// You can set name, ownerId, courseState, description, descriptionHeading, room, section.
        /// Returns the created course.
        /// </remarks>
        /// <response code="400">Invalid input data or Google api error.</response>
        /// <response code="401">Not authorized.</response>
        /// <response code="404">OwnerId not specified.</response>
        /// <response code="500">Credentials error.</response>
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
                if (e is GoogleApiException)
                {
                    _logger.LogError("An error was found when executing the request 'create'. {error}", e.Message);
                    return StatusCode(400, "Google api error.");
                }
                else if (e is AggregateException)
                {
                    _logger.LogError("An error was found when executing the request 'create'. {error}", e.Message);
                    return StatusCode(500, "Credentials error");
                }
                else
                {
                    _logger.LogError("An error was found when executing the request 'create'. {error}", e.Message);
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
        /// <response code="500">Credentials error.</response>
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
                    _logger.LogError("An error was found when executing the request 'archive{{courseId}}'. {error}", e.Message);
                    return StatusCode(404, "Course was not found.");
                }
                else if (errorResponse == HttpStatusCode.BadRequest)
                {
                    _logger.LogError("An error was found when executing the request 'archive{{courseId}}'. {error}", e.Message);
                    return StatusCode(400, "Unable to change course," +
                        " you should check that you are trying to change only the available fields");
                }

                _logger.LogError("An error was found when executing the request 'archive{{courseId}}'. {error}", e.Message);
                return StatusCode(520, "Unknown error");
            }
            catch (Exception e)
            {
                if (e is NullReferenceException)
                {
                    _logger.LogError("An error was found when executing the request 'archive{{courseId}}'. {error}", e.Message);
                    return StatusCode(404, "Course not found in database.");
                }
                else if (e is AggregateException)
                {
                    _logger.LogError("An error was found when executing the request 'archive{{courseId}}'. {error}", e.Message);
                    return StatusCode(500, "Credentials error.");
                }
                else
                {
                    _logger.LogError("An error was found when executing the request 'archive{{courseId}}'. {error}", e.Message);
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
        /// <response code="500">Credentials error.</response>
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
                    _logger.LogError("An error was found when executing the request 'patch{{courseId}}'. {error}", e.Message);
                    return StatusCode(404, "Course was not found.");
                }
                else if (errorResponse == HttpStatusCode.BadRequest)
                {
                    _logger.LogError("An error was found when executing the request 'patch{{courseId}}'. {error}", e.Message);
                    return StatusCode(400, "Unable to change course," +
                        " you should check that you are trying to change only the available fields.");
                }

                _logger.LogError("An error was found when executing the request 'patch{{courseId}}'. {error}", e.Message);
                return StatusCode(520, "Unknown error.");
            }
            catch (Exception e)
            {
                if (e is NullReferenceException)
                {
                    _logger.LogError("An error was found when executing the request 'patch{{courseId}}'. {error}", e.Message);
                    return StatusCode(404, "Course not found in database.");
                }
                else if (e is AggregateException)
                {
                    _logger.LogError("An error was found when executing the request 'patch{{courseId}}'. {error}", e.Message);
                    return StatusCode(500, "Credentials error.");
                }
                else
                {
                    _logger.LogError("An error was found when executing the request 'patch{{courseId}}'. {error}", e.Message);
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
        /// <response code="500">Credentials error.</response>
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
                    _logger.LogError("An error was found when executing the request 'update{{courseId}}'. {error}", e.Message);
                    return StatusCode(404, "Course was not found.");
                }
                else if (errorResponse == HttpStatusCode.BadRequest)
                {
                    _logger.LogError("An error was found when executing the request 'update{{courseId}}'. {error}", e.Message);
                    return StatusCode(400, "You are not permitted to modify this course or course is not modifable.");
                }

                _logger.LogError("An error was found when executing the request 'update{{courseId}}'. {error}", e.Message);
                return StatusCode(520, "Unknown error.");
            }
            catch (Exception e)
            {
                if (e is NullReferenceException)
                {
                    _logger.LogError("An error was found when executing the request 'update{{courseId}}'. {error}", e.Message);
                    return StatusCode(404, "Course not found in database.");
                }
                else if (e is AggregateException)
                {
                    _logger.LogError("An error was found when executing the request 'update{{courseId}}'. {error}", e.Message);
                    return StatusCode(500, "Credentials error.");
                }
                else
                {
                    _logger.LogError("An error was found when executing the request 'update{{courseId}}'. {error}", e.Message);
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
        /// <response code="500">Credentials error.</response>
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
                    _logger.LogError("An error was found when executing the request 'delete{{courseId}}'. {error}", e.Message);
                    return StatusCode(404, "Course was not found.");
                }
                else if (errorResponse == HttpStatusCode.BadRequest)
                {
                    _logger.LogError("An error was found when executing the request 'delete{{courseId}}'. {error}", e.Message);
                    return StatusCode(400, "Precondition check failed. Perhaps you should archive the course first.");
                }

                _logger.LogError("An error was found when executing the request 'delete{{courseId}}'. {error}", e.Message);
                return StatusCode(520, "Unknown error.");
            }
            catch (Exception e)
            {
                if (e is NullReferenceException)
                {
                    _logger.LogError("An error was found when executing the request 'delete{{courseId}}'. {error}", e.Message);
                    return StatusCode(404, "Course not found in database.");
                }
                else if (e is AggregateException)
                {
                    _logger.LogError("An error was found when executing the request 'delete{{courseId}}'. {error}", e.Message);
                    return StatusCode(500, "Credentials error.");
                }
                else
                {
                    _logger.LogError("An error was found when executing the request 'delete{{courseId}}'. {error}", e.Message);
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
        /// <response code="500">Credentials error.</response>
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
                if (e is GoogleApiException)
                {
                    _logger.LogError("An error was found when executing the request 'synchronize'. {error}", e.Message);
                    return StatusCode(400, "GoogleApi exception.");
                }
                if (e is AggregateException)
                {
                    _logger.LogError("An error was found when executing the request 'synchronize'. {error}", e.Message);
                    return StatusCode(500, "Credentials error.");
                }
                else
                {
                    _logger.LogError("An error was found when executing the request 'synchronize'. {error}", e.Message);
                    return StatusCode(520, "Unknown error.");
                }
            }
        }

        /// <summary>
        /// Create courses list.
        /// </summary>
        /// <remarks>
        /// Get array of courses names, returns task id.
        /// </remarks>
        [HttpPost("createList")]
        public async Task<IActionResult> CreateCoursesList([FromBody] List<string> courses)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(400, "Invalid input data.");
            }
            try
            {
                var task = await _coursesListService.CreateCoursesList(courses);
                return Ok(task);
            }
            catch (Exception e)
            {
                _logger.LogError("An error was found when executing the request 'createList'. {error}", e.Message);
                return StatusCode(520, "Unknown error.");
            }
        }

        /// <summary>
        /// Get task detailed info.
        /// </summary>
        /// <remarks>
        /// Returns detailed info about task.
        /// </remarks>
        [HttpGet("task/{id:int}")]
        public async Task<IActionResult> GetTaskInfo(int id)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(400, "Invalid input data.");
            }
            try
            {
                var response = await _coursesListService.GetTaskInfo(id);
                return Ok(response);
            }
            catch (Exception e)
            {
                _logger.LogError("An error was found when executing the request 'task/{id}'. {error}", id, e.Message);
                return StatusCode(520, "Unknown error.");
            }
        }

        /// <summary>
        /// Cancel task.
        /// </summary>
        /// <remarks>
        /// Deletes all courses that have already been created.
        /// </remarks>
        [HttpPost("cancelTask/{id:int}")]
        public async Task<IActionResult> CancelTask(int id)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(400, "Invalid input data.");
            }
            try
            {
                await _coursesListService.CancelTask(id);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError("An error was found when executing the request 'cancelTask/{id}'. {error}", id, e.Message);
                return StatusCode(520, "Unknown error.");
            }
        }

        /// <summary>
        /// Retry task.
        /// </summary>
        /// <remarks>
        /// Retry task if task.Status == WAITING_TO_CONTINUE.
        /// </remarks>
        [HttpPost("retryTask/{id:int}")]
        public async Task<IActionResult> RetryTask(int id)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(400, "Invalid input data.");
            }
            try
            {
                var result = await _coursesListService.RetryTask(id);
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError("An error was found when executing the request 'retryTask/{id}'. {error}", id, e.Message);
                return StatusCode(520, "Unknown error.");
            }
        }

        /// <summary>
        /// Find task's courses.
        /// </summary>
        /// <remarks>
        /// Returns real courses which were created for this task.
        /// </remarks>
        [HttpGet("taskCourses/{taskId:int}")]
        public async Task<IActionResult> GetTaskCourses(int taskId)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(400, "Invalid input data.");
            }
            try
            {
                var result = await _coursesListService.GetTaskCourses(taskId);
                return Ok(result);
            }
            catch(Exception e)
            {
                _logger.LogError("An error was found when executing the request 'taskCourses/{taskId}'. {error}", taskId, e.Message);
                return StatusCode(520, "Unknown error.");
            }
        }

        /// <summary>
        /// Find tasks info.
        /// </summary>
        /// <remarks>
        /// Returns short info about tasks related to the specified status.
        /// </remarks>
        [HttpGet("getTasks")]
        public async Task<IActionResult> GetTasks([FromQuery] string? status)
        {
            try
            {
                var result = await _coursesListService.GetTasks(status);
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError("An error was found when executing the request 'getTasks'. {error}", e.Message);
                return StatusCode(520, "Unknown error.");
            }
        }
    }
}
