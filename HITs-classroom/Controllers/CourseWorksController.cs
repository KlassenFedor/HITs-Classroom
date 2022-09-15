using Google;
using Google.Apis.Classroom.v1.Data;
using HITs_classroom.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
                Claim? relatedUser = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                if (relatedUser == null)
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'acces/course/{{courseId}}/courseWork/{{courseWorkId}}'. {error}", "Email not found.");
                    return StatusCode(401, "Unable to access your courses.");
                }
                _courseWorksService.SetAdmittedStudentsForCourseWork(courseId, courseWorkId, users, relatedUser.Value);
                return Ok();
            }
            catch (GoogleApiException e)
            {
                var errorResponse = e.HttpStatusCode;

                if (errorResponse == HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'acces/course/{{courseId}}/courseWork/{{courseWorkId}}'. {error}", e.Message);
                    return StatusCode(404, "Course was not found.");
                }
                else if (errorResponse == HttpStatusCode.BadRequest)
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'acces/course/{{courseId}}/courseWork/{{courseWorkId}}'. {error}", e.Message);
                    return StatusCode(400, "Unable to change course," +
                        " you should check that you are trying to change only the available fields");
                }

                return StatusCode(520, "Unknown error");
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'acces/course/{{courseId}}/courseWork/{{courseWorkId}}'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'acces/course/{{courseId}}/courseWork/{{courseWorkId}}'. {error}", e.Message);
                    return StatusCode(520, "Unknown error");
                }
            }
        }
    }
}
