using Google;
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using HITs_classroom.Enums;
using HITs_classroom.Models.Invitation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Text.Json;

namespace HITs_classroom.Services
{
    public interface IInvitationsService
    {
        Invitation CreateInvitation(InvitationManagementModel parameters);
        string DeleteInvitation(string id);
        Invitation GetInvitation(string id);
        Task<InvitationStatus> CheckInvitationStatus(string id);
    }

    public class InvitationsService: IInvitationsService
    {
        private GoogleClassroomService _googleClassroomService;
        private ApplicationDbContext _context;
        public InvitationsService(GoogleClassroomService googleClassroomService, ApplicationDbContext context)
        {
            _googleClassroomService = googleClassroomService;
            _context = context;
        }

        public Invitation CreateInvitation(InvitationManagementModel parameters)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService();
            Invitation newInvitation = new Invitation();
            newInvitation.UserId = parameters.UserId;
            newInvitation.CourseId = parameters.CourseId;
            newInvitation.Role = ((CourseRoleEnum)parameters.Role).ToString();

            var request = classroomService.Invitations.Create(newInvitation);
            var response = request.Execute();

            return response;
        }

        public string DeleteInvitation(string id)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService();
            var request = classroomService.Invitations.Delete(id);
            var response = request.Execute();

            return JsonSerializer.Serialize(response);
        }

        public Invitation GetInvitation(string id)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService();
            var request = classroomService.Invitations.Get(id);
            var response = request.Execute();

            return response;
        }

        public async Task<InvitationStatus> CheckInvitationStatus(string id)
        {
            var classroomService = _googleClassroomService.GetClassroomService();
            var invitation = await _context.Invitations.Where(i => i.Id == id).FirstOrDefaultAsync();
            if (invitation != null)
            {
                if (invitation.IsAccepted)
                {
                    return InvitationStatus.ACCEPTED;
                }

                InvitationManagementModel invitationParameters = new InvitationManagementModel
                {
                    CourseId = invitation.CourseId,
                    Role = invitation.Role,
                    UserId = invitation.UserId
                };

                try
                {
                    if (CheckIfUserAcceptedInvitaton(invitationParameters, classroomService))
                    {
                        invitation.IsAccepted = true;
                        await _context.SaveChangesAsync();
                        return InvitationStatus.ACCEPTED;
                    }
                    return InvitationStatus.NOT_ACCEPTED;
                }
                catch (Exception e)
                {
                    throw;
                }
            }
            return InvitationStatus.NOT_EXISTS;
        }

        private bool CheckIfUserAcceptedInvitaton(InvitationManagementModel invitation, ClassroomService classroomService)
        {
            if (invitation.Role == (int)CourseRoleEnum.STUDENT)
            {
                try
                {
                    var student = classroomService.Courses.Students.Get(invitation.CourseId, invitation.UserId).Execute();
                    if (student != null)
                    {
                        return true;
                    }
                    return false;
                }
                catch (GoogleApiException e)
                {
                    if (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return false;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else if (invitation.Role == (int)CourseRoleEnum.TEACHER)
            {
                try
                {
                    var teacher = classroomService.Courses.Teachers.Get(invitation.CourseId, invitation.UserId).Execute();
                    if (teacher != null)
                    {
                        return true;
                    }
                    return false;
                }
                catch (Exception e)
                {
                    return false;
                }
            }

            return false;
        }
    }
}
