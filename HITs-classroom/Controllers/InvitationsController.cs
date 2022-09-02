﻿using Google;
using HITs_classroom.Models.Invitation;
using HITs_classroom.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        /// id - invitation Id
        /// </remarks>
        /// <response code="403">You are not permitted to delete invitations for this course.</response>
        /// <response code="404">No invitation exists with the requested ID.</response>
        /// <response code="500">Credential Not found.</response>
        [HttpGet("get/{invitationId}")]
        public async Task<IActionResult> GetInvitation(string invitationId)
        {
            try
            {
                var result = await _invitationsService.GetInvitation(invitationId);
                return Ok(new JsonResult(result).Value);
            }
            catch (GoogleApiException e)
            {
                if (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("An error was found when executing the request 'get/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(404, "No invitation exists with the requested ID.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'get/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(403, "You are not permitted to get invitations for this course.");
                }
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request 'get/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
                }
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'get/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(520, "Unknown error.");
                }
            }
        }

        /// <summary>
        /// Сhecks the status of the invitation
        /// </summary>
        /// <remarks>
        /// id - invitation Id
        /// 
        /// Possible statuses: ACCEPTED, NOT_ACCEPTED, NOT_EXISTS (if the invitation is not found).
        /// </remarks>
        /// <response code="403">You are not permitted to check invitations for this course.Course does not exist.</response>
        /// <response code="500">Credential Not found.</response>
        [HttpGet("check/{invitationId}")]
        public async Task<IActionResult> СheckInvitationStatus(string invitationId)
        {
            try
            {
                var result = await _invitationsService.CheckInvitationStatus(invitationId);
                return Ok(result);
            }
            catch (GoogleApiException e)
            {
                _logger.LogInformation("An error was found when executing the request 'check/{{invitationId}}'. {error}", e.Message);
                return StatusCode(403, "You are not permitted to check invitations for this course.");
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request 'check/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
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
        /// <response code="403">You are not permitted to check invitations for this course.Course does not exist.</response>
        /// <response code="500">Credential Not found.</response>
        [HttpPost("update")]
        public async Task<IActionResult> UpdateAllInvitations()
        {
            try
            {
                await _invitationsService.UpdateAllInvitations();
                return Ok();
            }
            catch (GoogleApiException e)
            {
                _logger.LogInformation("An error was found when executing the request 'update'. {error}", e.Message);
                return StatusCode(403, "You are not permitted to update invitations for this course.");
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    _logger.LogInformation("An error was found when executing the request 'update'. {error}", e.Message);
                    return StatusCode(500, "Credential Not found.");
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
        /// id - course identifier.
        /// Сheck the invitations and updates the values of the field IsAccepted.
        /// </remarks>
        /// <response code="403">You are not permitted to check invitations for this course.Course does not exist.</response>
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
        /// <response code="403">You are not permitted to create invitations for this course
        /// or the requested users account is disabled or the user already has this role
        /// or a role with greater permissions.</response>
        /// <response code="404">Course or user does not exist.</response>
        /// <response code="409">Invitation already exists.</response>
        /// <response code="500">Credential Not found.</response>
        [HttpPost("create")]
        public async Task<IActionResult> CreateInvitation([FromBody] InvitationManagementModel parameters)
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
                        " the user already has this role or a role with greater permissions." + e.Message);
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
        /// id - invitation Id
        /// </remarks>
        /// <response code="403">You are not permitted to delete invitations for this course.</response>
        /// <response code="404">No invitation exists with the requested ID.</response>
        /// <response code="500">Credential Not found.</response>
        [HttpDelete("delete/{invitationId}")]
        public async Task<IActionResult> DeleteInvitation(string invitationId)
        {
            try
            {
                var result = await _invitationsService.DeleteInvitation(invitationId);
                return Ok("Successfully deleted.");
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
                else
                {
                    _logger.LogInformation("An error was found when executing the request 'delete/{{invitationId}}'. {error}", e.Message);
                    return StatusCode(520, "Unknown error.");
                }
            }
        }
    }
}
