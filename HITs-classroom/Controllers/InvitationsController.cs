using Google;
using HITs_classroom.Jobs;
using HITs_classroom.Models.Invitation;
using HITs_classroom.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HITs_classroom.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvitationsController : ControllerBase
    {
        private readonly IInvitationsService _invitationsService;
        private readonly ILogger _logger;
        public InvitationsController(IInvitationsService invitationsService, ILogger<InvitationsController> logger)
        {
            _invitationsService = invitationsService;
            _logger = logger;
        }

        /// <summary>
        /// Search for a invitation by id.
        /// </summary>
        /// <remarks>
        /// invitationId - invitation Identifier
        /// </remarks>
        /// <response code="401">Not authorized.</response>
        /// <response code="404">No invitation exists with the requested ID.</response>
        /// <response code="500">Credential Not found.</response>
        [Authorize]
        [HttpGet("get/{invitationId}")]
        public async Task<IActionResult> GetInvitation(string invitationId)
        {
            try
            {
                Claim? relatedUser = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                if (relatedUser == null)
                {
                    _logger.LogInformation("An error was found when executing the request 'get/{{invitationId}}'. {error}",
                        "Email not found.");
                    return StatusCode(401, "Not authorized.");
                }
                var result = await _invitationsService.GetInvitation(invitationId, relatedUser.Value);
                return Ok(new JsonResult(result).Value);
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request 'get/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
                }
                else if (e is NullReferenceException)
                {
                    _logger.LogInformation("An error was found when executing the request 'get/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(404, "No invitation exists with the requested ID.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'get/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(520, "Unknown error.");
                }
            }
        }

        /// <summary>
        /// Search for the course invitations.
        /// </summary>
        /// <remarks>
        /// courseId - course Identifier.
        /// </remarks>
        /// <response code="401">Not authorized.</response>
        /// <response code="404">No invitation exists with the requested course ID.</response>
        /// <response code="500">Credential Not found.</response>
        [Authorize]
        [HttpGet("list/{courseId}")]
        public async Task<IActionResult> GetCourseInvitations(string courseId)
        {
            try
            {
                Claim? relatedUser = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                if (relatedUser == null)
                {
                    _logger.LogInformation("An error was found when executing the request 'list/{{courseId}}'. {error}", "Email not found.");
                    return StatusCode(401, "Not authorized.");
                }
                var result = await _invitationsService.GetCourseInvitations(courseId, relatedUser.Value);
                return Ok(new JsonResult(result).Value);
            }
            catch (GoogleApiException e)
            {
                if (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("An error was found when executing the request 'list/{{courseId}}'. {error}", e.Message);
                    return StatusCode(404, "No invitation exists with the requested ID.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'list/{{courseId}}'. {error}", e.Message);
                    return StatusCode(403, "You are not permitted to get invitations for this course.");
                }
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request 'list/{{courseId}}'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'list/{{courseId}}'. {error}", e.Message);
                    return StatusCode(520, "Unknown error.");
                }
            }
        }

        /// <summary>
        /// Сhecks the status of the invitation
        /// </summary>
        /// <remarks>
        /// invitationId - invitation Identifier
        /// 
        /// Possible statuses: ACCEPTED, NOT_ACCEPTED, NOT_EXISTS (if the invitation is not found).
        /// </remarks>
        /// <response code="401">Could not access the user's email.</response>
        /// <response code="403">You are not permitted to check invitations for this course.Course does not exist.</response>
        /// <response code="500">Credential Not found.</response>
        [Authorize]
        [HttpGet("check/{invitationId}")]
        public async Task<IActionResult> СheckInvitationStatus(string invitationId)
        {
            try
            {
                Claim? relatedUser = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                if (relatedUser == null)
                {
                    _logger.LogInformation("An error was found when executing the request 'check/{{invitationId}}'. {error}",
                        "Email not found.");
                    return StatusCode(401, "Unable to access your courses.");
                }
                var result = await _invitationsService.CheckInvitationStatus(invitationId, relatedUser.Value);
                return Ok(result);
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request 'check/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
                }
                else if (e is ArgumentException)
                {
                    _logger.LogInformation("An error was found when executing the request 'check/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(403, "You are not allowed to get this invitation.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'check/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(520, "Unknown error.");
                }
            }
        }

        /// <summary>
        /// Update the status of the all invitations
        /// </summary>
        /// <remarks>
        /// Сheck all the invitations and updates the value of the field IsAccepted.
        /// </remarks>
        /// <response code="401">Could not access the user's email.</response>
        /// <response code="403">You are not permitted to check invitations for this course.Course does not exist.</response>
        /// <response code="500">Credential Not found.</response>
        [Authorize]
        [HttpPost("updateAll")]
        public async Task<IActionResult> UpdateAllInvitations()
        {
            try
            {
                Claim? relatedUser = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                if (relatedUser == null)
                {
                    _logger.LogInformation("An error was found when executing the request 'update'. {error}",
                        "Email not found.");
                    return StatusCode(401, "Unable to access your courses.");
                }
                InvitationsScheduler.Start(relatedUser.Value);
                return Ok();
            }
            catch (GoogleApiException e)
            {
                _logger.LogInformation("An error was found when executing the request 'update'. {error}", e.Message);
                return StatusCode(403, "You are not permitted to update invitations.");
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request 'update'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
                }
                else if (e is ArgumentException)
                {
                    _logger.LogInformation("An error was found when executing the request 'update'. {error}", e.Message);
                    return StatusCode(403, "You are not permitted to update invitations.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'update'. {error}", e.Message);
                    return StatusCode(520, "Unknown error.");
                }
            }
        }

        /// <summary>
        /// Update the statuses of the invitations
        /// </summary>
        /// <remarks>
        /// courseId - course identifier.
        /// Сheck the invitations and updates the values of the field IsAccepted.
        /// </remarks>
        /// <response code="401">Could not access the user's email.</response>
        /// <response code="403">You are not permitted to check invitations for this course.Course does not exist.</response>
        /// <response code="500">Credential Not found.</response>
        [Authorize]
        [HttpPost("update/{courseId}")]
        public async Task<IActionResult> UpdateCourseIvitations(string courseId)
        {
            try
            {
                Claim? relatedUser = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                if (relatedUser == null)
                {
                    _logger.LogInformation("An error was found when executing the request 'update{{courseId}}'. {error}",
                        "Email not found.");
                    return StatusCode(401, "Unable to access your courses.");
                }
                await _invitationsService.UpdateCourseInvitations(courseId, relatedUser.Value);
                return Ok();
            }
            catch (GoogleApiException e)
            {
                _logger.LogInformation("An error was found when executing the request 'update{{courseId}}'. {error}", e.Message);
                return StatusCode(403, "You are not permitted to update invitations for this course.");
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request 'update{{courseId}}'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
                }
                else if (e is ArgumentException)
                {
                    _logger.LogInformation("An error was found when executing the request 'update'. {error}", e.Message);
                    return StatusCode(403, "You are not permitted to update invitations for this course.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'update{{courseId}}'. {error}", e.Message);
                    return StatusCode(520, "Unknown error.");
                }
            }
        }

        /// <summary>
        /// Creates the invitation
        /// </summary>
        /// <remarks>
        /// courseId - classroom-assigned identifier or an alias (if exists).
        /// userId - identifier of the invited user.
        /// role - role to invite the user to have. (STUDENT = 1, TEACHER = 2, OWNER = 3)
        /// </remarks>
        /// <response code="400">Invalid input data.</response>
        /// <response code="401">Could not access the user's email.</response>
        /// <response code="403">You are not permitted to create invitations for this course.</response>
        /// <response code="404">Course or user does not exist.</response>
        /// <response code="409">Invitation already exists.</response>
        /// <response code="500">Credential Not found.</response>
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateInvitation([FromBody] InvitationCreatingModel parameters)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(400, "Invalid input data.");
            }
            try
            {
                Claim? relatedUser = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                if (relatedUser == null)
                {
                    _logger.LogInformation("An error was found when executing the request 'create'. {error}",
                        "Email not found.");
                    return StatusCode(401, "Unable to access your courses.");
                }
                var invitation = await _invitationsService.CreateInvitation(parameters, relatedUser.Value);
                return new JsonResult(invitation);
            }
            catch (GoogleApiException e)
            {
                if (e.HttpStatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    _logger.LogInformation("An error was found when executing the request 'create'. {error}", e.Message);
                    return StatusCode(409, "Invitation already exists.");
                }
                else if (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("An error was found when executing the request 'create'. {error}", e.Message);
                    return StatusCode(404, "Course or user does not exist.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'create'. {error}", e.Message);
                    return StatusCode(403, "You are not permitted to create invitations for this course" +
                        " or the requested users account is disabled or" +
                        " the user already has this role or a role with greater permissions.");
                }
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request 'create'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'create'. {error}", e.Message);
                    return StatusCode(520, "Unknown error.");
                }
            }
        }


        /// <summary>
        /// Deletes the invitation
        /// </summary>
        /// <remarks>
        /// invitationId - invitation Identifier
        /// </remarks>
        /// <response code="401">Could not access the user's email.</response>
        /// <response code="403">You are not permitted to delete invitations for this course.</response>
        /// <response code="404">No invitation exists with the requested ID.</response>
        /// <response code="500">Credential Not found.</response>
        [Authorize]
        [HttpDelete("delete/{invitationId}")]
        public async Task<IActionResult> DeleteInvitation(string invitationId)
        {
            try
            {
                Claim? relatedUser = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                if (relatedUser == null)
                {
                    _logger.LogInformation("An error was found when executing the request 'delete/{{invitationId}}'. {error}",
                        "Email not found.");
                    return StatusCode(401, "Unable to access your courses.");
                }
                await _invitationsService.DeleteInvitation(invitationId, relatedUser.Value);
                return Ok(new JsonResult("Successfully deleted."));
            }
            catch (GoogleApiException e)
            {
                if (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("An error was found when executing the request 'delete/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(404, "No invitation exists with the requested ID.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'delete/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(403, "You are not permitted to delete invitations for this course.");
                }
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request 'delete/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
                }
                else if (e is NullReferenceException)
                {
                    _logger.LogInformation("An error was found when executing the request 'delete/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(404, "No invitation exists with the requested ID.");
                }
                else if (e is ArgumentException)
                {
                    _logger.LogInformation("An error was found when executing the request 'delete/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(403, "You are not allowed to delete this invitation.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'delete/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(520, "Unknown error.");
                }
            }
        }

        /// <summary>
        /// Resends the invitation
        /// </summary>
        /// <remarks>
        /// invitationId - invitation Identifier
        /// </remarks>
        /// <response code="401">Could not access the user's email.</response>
        /// <response code="403">You are not permitted to resend invitations for this course.</response>
        /// <response code="404">Invitation does not exist.</response>
        /// <response code="409">Invitation already exists.</response>
        /// <response code="500">Credential Not found.</response>
        [Authorize]
        [HttpPost("resend/{invitationId}")]
        public async Task<IActionResult> ResendInvitation(string invitationId)
        {
            try
            {
                Claim? relatedUser = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                if (relatedUser == null)
                {
                    _logger.LogInformation("An error was found when executing the request 'resend/{{invitationId}}'. {error}",
                        "Email not found.");
                    return StatusCode(401, "Unable to access your courses.");
                }
                await _invitationsService.ResendInvitation(invitationId, relatedUser.Value);
                return Ok();
            }
            catch (GoogleApiException e)
            {
                if (e.HttpStatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    _logger.LogInformation("An error was found when executing the request 'resend/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(409, "Invitation already exists.");
                }
                else if (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("An error was found when executing the request 'resend/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(404, "Invitation does not exist.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'resend/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(403, "You are not permitted to create invitations for this course" +
                        " or the requested users account is disabled or" +
                        " the user already has this role or a role with greater permissions." + e.Message);
                }
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request 'resend/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
                }
                else if (e is NullReferenceException)
                {
                    _logger.LogInformation("An error was found when executing the request 'resend/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(404, "No invitation exists with the requested ID.");
                }
                else if (e is ArgumentException)
                {
                    _logger.LogInformation("An error was found when executing the request 'resend/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(403, "You are not allowed to resend this invitation.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'resend/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(520, "Unknown error.");
                }
            }
        }

        /// <summary>
        /// Checks teachers invitations
        /// </summary>
        /// <remarks>
        /// courseId - course Identifier
        /// 
        /// Сhecks if all teachers have accepted the invitations.
        /// </remarks>
        /// <response code="401">Could not access the user's email.</response>
        /// <response code="403">You are not allowed to check teachers for this course.</response>
        /// <response code="404">Couldn't find a course.</response>
        [Authorize]
        [HttpGet("checkTeachersInvitations/{courseId}")]
        public async Task<IActionResult> CheckTeachersInvitations(string courseId)
        {
            try
            {
                Claim? relatedUser = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                if (relatedUser == null)
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'checkTeachersInvitations/{{courseId}}'. {error}",
                        "Email not found.");
                    return StatusCode(401, "Unable to access your courses.");
                }
                var response = await _invitationsService.CheckIfAllTeachersAcceptedInvitations(courseId, relatedUser.Value);
                return new JsonResult(response);
            }
            catch (Exception e)
            {
                if (e is NullReferenceException)
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'checkTeachersInvitations/{{courseId}}'. {error}", e.Message);
                    return StatusCode(404, "Couldn't find a course.");
                }
                else if (e is ArgumentException)
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'checkTeachersInvitations/{{courseId}}'. {error}", e.Message);
                    return StatusCode(403, "You are not allowed to check teachers for this course.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request" +
                        " 'checkTeachersInvitations/{{courseId}}'. {error}", e.Message);
                    return StatusCode(520, "Unknown error.");
                }
            }
        }
    }
}
