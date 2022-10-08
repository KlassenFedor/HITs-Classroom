using Google;
using Google.Apis.Classroom.v1;
using HITs_classroom.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace HITs_classroom.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseWorksController : ControllerBase
    {
        private readonly ICourseWorksService _courseWorksService;
        private readonly ILogger _logger;
        public CourseWorksController(ICourseWorksService courseWorksService, ILogger<CourseWorksController> logger)
        {
            _courseWorksService = courseWorksService;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("acces/course/{courseId}/courseWork/{courseWorkId}")]
        public IActionResult SetAdmittedStudentsToCourseWork(string courseId, string courseWorkId, [FromBody] List<string>? users)
        {
            try
            {
                _courseWorksService.SetAdmittedStudentsForCourseWork(courseId, courseWorkId, users);
                return Ok();
            }
            catch (GoogleApiException e)
            {
                var errorResponse = e.HttpStatusCode;

                if (errorResponse == HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'acces/course/{{courseId}}/courseWork/{{courseWorkId}}'. {error}", e.Message);
                    return StatusCode(404, "Not found.");
                }
                else if (errorResponse == HttpStatusCode.BadRequest)
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'acces/course/{{courseId}}/courseWork/{{courseWorkId}}'. {error}", e.Message);
                    return StatusCode(400, "Failed precondition.");
                }

                _logger.LogInformation("An error was found when executing the request" +
                        " 'acces/course/{{courseId}}/courseWork/{{courseWorkId}}'. {error}", e.Message);
                return StatusCode(520, "Unknown error");
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'acces/course/{{courseId}}/courseWork/{{courseWorkId}}'. {error}", e.Message);
                    return StatusCode(500, "Credentials error.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'acces/course/{{courseId}}/courseWork/{{courseWorkId}}'. {error}", e.Message);
                    return StatusCode(520, "Unknown error");
                }
            }
        }

        /// <summary>
        /// Receive grades for all course works.
        /// </summary>
        /// <remarks>
        /// Sends a list of all users' grades for all course works.
        /// </remarks>
        /// <response code="400">Unable to get course grades.</response>
        /// <response code="401">Not authorized.</response>
        /// <response code="404">Course was not found.</response>
        /// <response code="500">Credential Not found.</response>
        [Authorize]
        [HttpGet("courseGrades/{courseId}")]
        public async Task<IActionResult> GetCourseGrades(string courseId)
        {
            try
            {
                var response = await _courseWorksService.GetCourseGrades(courseId);
                return Ok(new JsonResult(response).Value);
            }
            catch (GoogleApiException e)
            {
                if (e.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'courseGrades/{{courseId}}'. {error}", e.Message);
                    return StatusCode(404, "Course not found.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'courseGrades/{{courseId}}'. {error}", e.Message);
                    return StatusCode(400, "Unable to get course grades.");
                }
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'courseGrades/{{courseId}}'. {error}", e.Message);
                    return StatusCode(500, "Credentials error.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'courseGrades/{{courseId}}'. {error}", e.Message);
                    return StatusCode(520, "Unknown error");
                }
            }
        }

        /// <summary>
        /// Receive grades for all course works.
        /// </summary>
        /// <remarks>
        /// Sends a list of all course works.
        /// </remarks>
        /// <response code="400">Unable to get course works.</response>
        /// <response code="401">Not authorized.</response>
        /// <response code="404">Course was not found.</response>
        /// <response code="500">Credential Not found.</response>
        [Authorize]
        [HttpGet("courseWorks/{courseId}")]
        public async Task<IActionResult> GetCourseWorks(string courseId)
        {
            try
            {
                var response = await _courseWorksService.GetCourseWorks(courseId);
                return Ok(new JsonResult(response).Value);
            }
            catch (GoogleApiException e)
            {
                if (e.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'courseWorks/{{courseId}}'. {error}", e.Message);
                    return StatusCode(404, "Course not found.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'courseWorks/{{courseId}}'. {error}", e.Message);
                    return StatusCode(400, "Unable to get course grades.");
                }
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'courseWorks/{{courseId}}'. {error}", e.Message);
                    return StatusCode(500, "Credentials error.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'courseWorks/{{courseId}}'. {error}", e.Message);
                    return StatusCode(520, "Unknown error");
                }
            }
        }
    }
}
