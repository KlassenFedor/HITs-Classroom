using Google;
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

        [HttpPost]
        public IActionResult CreateInvitation()
        {
            try
            {
                var invitation = _invitationsService.CreateInvitation();
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
    }
}
