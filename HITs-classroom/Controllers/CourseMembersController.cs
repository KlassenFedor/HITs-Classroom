﻿using Google;
using Google.Apis.Classroom.v1.Data;
using HITs_classroom.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HITs_classroom.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseMembersController : ControllerBase
    {
        private readonly ICourseMembersService _courseMembersService;
        private readonly ILogger _logger;
        public CourseMembersController(ICourseMembersService courseMembersService, ILogger<CoursesController> logger)
        {
            _courseMembersService = courseMembersService;
            _logger = logger;
        }

        /// <summary>
        /// Get the list of the students for the specified course.
        /// </summary>
        /// <remarks>
        /// courseId - course Identifier.
        /// </remarks>
        /// <response code="403">You are not allowed to get students.</response>
        /// <response code="404">Course does not exist.</response>
        /// <response code="500">Credential Not found.</response>
        [HttpGet("students/list/{courseId}")]
        public IActionResult GetStudentsList(string courseId)
        {
            try
            {
                var result = _courseMembersService.GetStudentsList(courseId);
                return new JsonResult(result);
            }
            catch (GoogleApiException e)
            {
                if (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'students/list/{{courseId}}'. {error}", e.Message);
                    return StatusCode(404, "Course does not exist.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'students/list/{{courseId}}'. {error}", e.Message);
                    return StatusCode(403, "You are not allowed to get students.");
                }
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'students/list/{{courseId}}'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'students/list/{{courseId}}'. {error}", e.Message);
                    return StatusCode(520, "Unknown error.");
                }
            }
        }

        /// <summary>
        /// Get the list of the teachers for the specified course.
        /// </summary>
        /// <remarks>
        /// courseId - course Identifier.
        /// </remarks>
        /// <response code="403">You are not allowed to get teachers.</response>
        /// <response code="404">Course does not exist.</response>
        /// <response code="500">Credential Not found.</response>
        [HttpGet("teachers/list/{courseId}")]
        public IActionResult GetTeachersList(string courseId)
        {
            try
            {
                var result = _courseMembersService.GetTeachersList(courseId);
                if (result.Count == 0)
                {
                    return new JsonResult(new Object());
                }
                return new JsonResult(result);
            }
            catch (GoogleApiException e)
            {
                if (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'teachers/list/{{courseId}}'. {error}", e.Message);
                    return StatusCode(404, "Course does not exist.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'teachers/list/{{courseId}}'. {error}", e.Message);
                    return StatusCode(403, "You are not allowed to get teachers.");
                }
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'teachers/list/{{courseId}}'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'teachers/list/{{courseId}}'. {error}", e.Message);
                    return StatusCode(520, "Unknown error.");
                }
            }
        }

        /// <summary>
        /// Delete the specified student for the specified course.
        /// </summary>
        /// <remarks>
        /// courseId - course Identifier.
        /// 
        /// studentId - course student Identifier.
        /// </remarks>
        /// <response code="403">You are not allowed to delete this student.</response>
        /// <response code="404">Course does not exist.</response>
        /// <response code="500">Credential Not found.</response>
        [HttpDelete("delete/courses/{courseId}/students/{studentId}")]
        public IActionResult DeleteStudent(string courseId, string studentId)
        {
            try
            {
                _courseMembersService.DeleteStudent(courseId, studentId);
                return Ok();
            }
            catch (GoogleApiException e)
            {
                if (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'delete/courses/{{courseId}}/students/{{studentId}}'. {error}", e.Message);
                    return StatusCode(404, "Course does not exist.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'delete/courses/{{courseId}}/students/{{studentId}}'. {error}", e.Message);
                    return StatusCode(403, "You are not allowed to delete this student.");
                }
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'delete/courses/{{courseId}}/students/{{studentId}}'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'delete/courses/{{courseId}}/students/{{studentId}}'. {error}", e.Message);
                    return StatusCode(520, "Unknown error.");
                }
            }
        }

        /// <summary>
        /// Delete the specified teacher for the specified course.
        /// </summary>
        /// <remarks>
        /// courseId - course Identifier.
        /// 
        /// teacherId - course teacher Identifier.
        /// </remarks>
        /// <response code="403">You are not allowed to delete this teacher.</response>
        /// <response code="404">Course does not exist.</response>
        /// <response code="500">Credential Not found.</response>
        [HttpDelete("delete/courses/{courseId}/teachers/{teacherId}")]
        public IActionResult DeleteTeacher(string courseId, string teacherId)
        {
            try
            {
                _courseMembersService.DeleteTeacher(courseId, teacherId);
                return Ok();
            }
            catch (GoogleApiException e)
            {
                if (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'delete/courses/{{courseId}}/teachers/{{teacherId}}'. {error}", e.Message);
                    return StatusCode(404, "Course does not exist.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'delete/courses/{{courseId}}/teachers/{{teacherId}}'. {error}", e.Message);
                    return StatusCode(403, "You are not allowed to delete this teacher.");
                }
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'delete/courses/{{courseId}}/teachers/{{teacherId}}'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'delete/courses/{{courseId}}/teachers/{{teacherId}}'. {error}", e.Message);
                    return StatusCode(520, "Unknown error.");
                }
            }
        }
    }
}
