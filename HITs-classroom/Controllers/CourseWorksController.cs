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
                Claim? relatedUser = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                if (relatedUser == null)
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'acces/course/{{courseId}}/courseWork/{{courseWorkId}}'. {error}", "Email not found.");
                    return StatusCode(401, "Unauthorized");
                }
                ClassroomService classroomService = new GoogleClassroomService().GetClassroomService(relatedUser.Value);
                _courseWorksService.SetAdmittedStudentsForCourseWork(courseId, courseWorkId, users, classroomService);
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
