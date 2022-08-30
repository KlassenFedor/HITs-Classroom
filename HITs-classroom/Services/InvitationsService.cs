using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using HITs_classroom.Enums;
using HITs_classroom.Models.Invitation;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Text.Json;

namespace HITs_classroom.Services
{
    public interface IInvitationsService
    {
        Invitation CreateInvitation(InvitationManagementModel parameters);
        string DeleteInvitation(string id);
        Invitation GetInvitation(string id);
        string CheckInvitationStatus(string id);
    }

    public class InvitationsService: IInvitationsService
    {
        private GoogleClassroomService _googleClassroomService;
        public InvitationsService(GoogleClassroomService googleClassroomService)
        {
            _googleClassroomService = googleClassroomService;
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

        public string CheckInvitationStatus(string id)
        {
            return null;
        }
    }
}
