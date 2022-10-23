using Google;
using HITs_classroom.Jobs;
using HITs_classroom.Models.Invitation;
using HITs_classroom.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HITs_classroom.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        [HttpGet("get/{invitationId}")]
        public async Task<IActionResult> GetInvitation(string invitationId)
        {
            try
            {
                var result = await _invitationsService.GetInvitation(invitationId);
                return Ok(new JsonResult(result).Value);
            }
            catch (Exception e)
            {
                if (e is NullReferenceException)
                {
                    _logger.LogError("An error was found when executing the request 'get/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(404, "No invitation exists with the requested ID.");
                }
                else
                {
                    _logger.LogError("An error was found when executing the request 'get/{{invitationId}}'. {error}", e.Message);
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
        [HttpGet("list/{courseId}")]
        public async Task<IActionResult> GetCourseInvitations(string courseId)
        {
            try
            {
                var result = await _invitationsService.GetCourseInvitations(courseId);
                return Ok(new JsonResult(result).Value);
            }
            catch (GoogleApiException e)
            {
                if (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogError("An error was found when executing the request 'list/{{courseId}}'. {error}", e.Message);
                    return StatusCode(404, "No invitation exists with the requested ID.");
                }
                else
                {
                    _logger.LogError("An error was found when executing the request 'list/{{courseId}}'. {error}", e.Message);
                    return StatusCode(403, "You are not permitted to get invitations for this course.");
                }
            }
            catch (Exception e)
            {
                _logger.LogError("An error was found when executing the request 'list/{{courseId}}'. {error}", e.Message);
                return StatusCode(520, "Unknown error.");
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
        /// <response code="401">Not authorized.</response>
        /// <response code="403">You are not permitted to check invitations for this course.Course does not exist.</response>
        /// <response code="500">Credentials error.</response>
        [HttpGet("check/{invitationId}")]
        public async Task<IActionResult> СheckInvitationStatus(string invitationId)
        {
            try
            {
                var result = await _invitationsService.CheckInvitationStatus(invitationId);
                return Ok(result);
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogError("An error was found when executing the request 'check/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(500, "Credentials error.");
                }
                else
                {
                    _logger.LogError("An error was found when executing the request 'check/{{invitationId}}'. {error}", e.Message);
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
        /// <response code="401">Not authorized.</response>
        /// <response code="403">You are not permitted to update invitations.</response>
        /// <response code="500">Credentials error.</response>
        [HttpPost("updateAll")]
        public async Task<IActionResult> UpdateAllInvitations()
        {
            try
            {
                InvitationsScheduler.Start();
                return Ok();
            }
            catch (GoogleApiException e)
            {
                _logger.LogError("An error was found when executing the request 'update'. {error}", e.Message);
                return StatusCode(403, "You are not permitted to update invitations.");
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogError("An error was found when executing the request 'update'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
                }
                else
                {
                    _logger.LogError("An error was found when executing the request 'update'. {error}", e.Message);
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
        /// <response code="401">Not authorized.</response>
        /// <response code="403">You are not permitted to update invitations for this course.</response>
        /// <response code="500">Credential Not found.</response>
        [HttpPost("update/{courseId}")]
        public async Task<IActionResult> UpdateCourseIvitations(string courseId)
        {
            try
            {
                await _invitationsService.UpdateCourseInvitations(courseId);
                return Ok();
            }
            catch (GoogleApiException e)
            {
                _logger.LogError("An error was found when executing the request 'update{{courseId}}'. {error}", e.Message);
                return StatusCode(403, "You are not permitted to update invitations for this course.");
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogError("An error was found when executing the request 'update{{courseId}}'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
                }
                else
                {
                    _logger.LogError("An error was found when executing the request 'update{{courseId}}'. {error}", e.Message);
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
        /// <response code="401">Not authorized.</response>
        /// <response code="403">You are not permitted to create invitations for this course.</response>
        /// <response code="404">Course or user does not exist.</response>
        /// <response code="409">Invitation already exists.</response>
        /// <response code="500">Credentials error.</response>
        [HttpPost("create")]
        public async Task<IActionResult> CreateInvitation([FromBody] InvitationCreatingModel parameters)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(400, "Invalid input data.");
            }
            try
            {
                var invitation = await _invitationsService.CreateInvitation(parameters);
                return new JsonResult(invitation);
            }
            catch (GoogleApiException e)
            {
                if (e.HttpStatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    _logger.LogError("An error was found when executing the request 'create'. {error}", e.Message);
                    return StatusCode(409, "Invitation already exists.");
                }
                else if (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogError("An error was found when executing the request 'create'. {error}", e.Message);
                    return StatusCode(404, "Course or user does not exist.");
                }
                else
                {
                    _logger.LogError("An error was found when executing the request 'create'. {error}", e.Message);
                    return StatusCode(403, "You are not permitted to create invitations for this course" +
                        " or the requested users account is disabled or" +
                        " the user already has this role or a role with greater permissions.");
                }
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogError("An error was found when executing the request 'create'. {error}", e.Message);
                    return StatusCode(500, "Credentials error.");
                }
                else
                {
                    _logger.LogError("An error was found when executing the request 'create'. {error}", e.Message);
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
        /// <response code="401">Not authorized.</response>
        /// <response code="403">You are not permitted to delete invitations for this course.</response>
        /// <response code="404">No invitation exists with the requested ID.</response>
        /// <response code="500">Credentials error.</response>
        [HttpDelete("delete/{invitationId}")]
        public async Task<IActionResult> DeleteInvitation(string invitationId)
        {
            try
            {
                await _invitationsService.DeleteInvitation(invitationId);
                return Ok(new JsonResult("Successfully deleted."));
            }
            catch (GoogleApiException e)
            {
                if (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogError("An error was found when executing the request 'delete/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(404, "No invitation exists with the requested ID.");
                }
                else
                {
                    _logger.LogError("An error was found when executing the request 'delete/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(403, "You are not permitted to delete invitations for this course.");
                }
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogError("An error was found when executing the request 'delete/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(500, "Credentials error.");
                }
                else if (e is NullReferenceException)
                {
                    _logger.LogError("An error was found when executing the request 'delete/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(404, "No invitation exists with the requested ID.");
                }
                else
                {
                    _logger.LogError("An error was found when executing the request 'delete/{{invitationId}}'. {error}", e.Message);
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
        /// <response code="401">Not authorized.</response>
        /// <response code="403">You are not permitted to resend invitations for this course.</response>
        /// <response code="404">Invitation does not exist.</response>
        /// <response code="500">Credentials error.</response>
        [HttpPost("resend/{invitationId}")]
        public async Task<IActionResult> ResendInvitation(string invitationId)
        {
            try
            {
                await _invitationsService.ResendInvitation(invitationId);
                return Ok();
            }
            catch (GoogleApiException e)
            {
                if (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogError("An error was found when executing the request 'resend/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(404, "Invitation does not exist.");
                }
                else
                {
                    _logger.LogError("An error was found when executing the request 'resend/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(403, "You are not permitted to create invitations for this course" +
                        " or the requested users account is disabled or" +
                        " the user already has this role or a role with greater permissions." + e.Message);
                }
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogError("An error was found when executing the request 'resend/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(500, "Credentials error.");
                }
                else if (e is NullReferenceException)
                {
                    _logger.LogError("An error was found when executing the request 'resend/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(404, "No invitation exists with the requested ID.");
                }
                else
                {
                    _logger.LogError("An error was found when executing the request 'resend/{{invitationId}}'. {error}", e.Message);
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
        /// <response code="401">Not authorized.</response>
        /// <response code="403">You are not allowed to check teachers for this course.</response>
        /// <response code="404">Couldn't find a course.</response>
        /// <response code="500">Credentials error.</response>
        [HttpGet("checkTeachersInvitations/{courseId}")]
        public async Task<IActionResult> CheckTeachersInvitations(string courseId)
        {
            try
            {
                var response = await _invitationsService.CheckIfAllTeachersAcceptedInvitations(courseId);
                return new JsonResult(response);
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogError("An error was found when executing the request 'checkTeachersInvitations/{{courseId}}'. {error}", e.Message);
                    return StatusCode(500, "Credentials error.");
                }
                else if (e is GoogleApiException)
                {
                    _logger.LogError("An error was found when executing the request 'checkTeachersInvitations/{{courseId}}'. {error}", e.Message);
                    return StatusCode(403, "You are not allowed to check teachers for this course.");
                }
                else if (e is NullReferenceException)
                {
                    _logger.LogError("An error was found when executing the request" +
                        " 'checkTeachersInvitations/{{courseId}}'. {error}", e.Message);
                    return StatusCode(404, "Couldn't find a course.");
                }
                else
                {
                    _logger.LogError("An error was found when executing the request" +
                        " 'checkTeachersInvitations/{{courseId}}'. {error}", e.Message);
                    return StatusCode(520, "Unknown error.");
                }
            }
        }
    }
}
