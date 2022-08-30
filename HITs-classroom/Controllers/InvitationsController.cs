using Google;
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
        public InvitationsController(IInvitationsService invitationsService)
        {
            _invitationsService = invitationsService;
        }

        /// <summary>
        /// Creates the invitation
        /// </summary>
        /// <remarks>
        /// courseId - classroom-assigned identifier or an alias (if exists).
        /// userId - identifier of the invited user.
        /// role - role to invite the user to have.
        /// </remarks>
        /// <response code="400">Invalid input data.</response>
        /// <response code="403">You are not permitted to create invitations for this course
        /// or the requested user's account is disabled or the user already has this role
        /// or a role with greater permissions.</response>
        /// <response code="404">Course or user does not exist.</response>
        /// <response code="409">Invitation already exists.</response>
        /// <response code="500">Credential Not found.</response>
        [HttpPost("create")]
        public IActionResult CreateInvitation([FromBody] InvitationManagementModel parameters)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(400, "Invalid input data.");
            }
            try
            {
                var invitation = _invitationsService.CreateInvitation(parameters);
                return new JsonResult(invitation);
            }
            catch (GoogleApiException e)
            {
                if (e.HttpStatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    return StatusCode(409, "Invitation already exists.");
                }
                else if (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return StatusCode(404, "Course or user does not exist.");
                }
                else
                {
                    return StatusCode(403, "You are not permitted to create invitations for this course" +
                        " or the requested user's account is disabled or" +
                        " the user already has this role or a role with greater permissions.");
                }
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    return StatusCode(500, "Credential Not found.");
                }
                else 
                {
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
        [HttpDelete("delete/{id}")]
        public IActionResult DeleteInvitation(string id)
        {
            try
            {
                var result = _invitationsService.DeleteInvitation(id);
                return Ok("Successfully deleted.");
            }
            catch (GoogleApiException e)
            {
                if (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return StatusCode(404, "No invitation exists with the requested ID.");
                }
                else
                {
                    return StatusCode(403, "You are not permitted to delete invitations for this course.");
                }
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    return StatusCode(500, "Credential Not found.");
                }
                else
                {
                    return StatusCode(520, "Unknown error.");
                }
            }
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
        [HttpGet("{id:int}")]
        public IActionResult GetInvitation(string id)
        {
            try
            {
                var result = _invitationsService.GetInvitation(id);
                return Ok(new JsonResult(result));
            }
            catch (GoogleApiException e)
            {
                if (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return StatusCode(404, "No invitation exists with the requested ID.");
                }
                else
                {
                    return StatusCode(403, "You are not permitted to get invitations for this course.");
                }
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    return StatusCode(500, "Credential Not found.");
                }
                else
                {
                    return StatusCode(520, "Unknown error.");
                }
            }
        }

        /// <summary>
        /// Сhecks the status of the invitation
        /// </summary>
        /// <remarks>
        /// id - invitation Id
        /// Possible statuses: ACCEPTED, NOT_ACCEPTED, NOT_EXISTS (if the invitation is not found).
        /// </remarks>
        /// <response code="403">You are not permitted to check invitations for this course.Course does not exist.</response>
        /// <response code="500">Credential Not found.</response>
        [HttpPost("{id}")]
        public IActionResult СheckInvitationStatus(string id)
        {
            try
            {
                var result = _invitationsService.CheckInvitationStatus(id);
                return Ok(result);
            }
            catch (GoogleApiException e)
            {
                return StatusCode(403, "You are not permitted to check invitations for this course.");
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    return StatusCode(500, "Credential Not found.");
                }
                else
                {
                    return StatusCode(520, "Unknown error.");
                }
            }
        }
    }
}
