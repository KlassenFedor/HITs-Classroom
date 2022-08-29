using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using HITs_classroom.Models.Invitation;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace HITs_classroom.Services
{
    public interface IInvitationsService
    {
        Invitation CreateInvitation();
    }

    public class InvitationsService: IInvitationsService
    {
        private GoogleClassroomService _googleClassroomService;
        public InvitationsService(GoogleClassroomService googleClassroomService)
        {
            _googleClassroomService = googleClassroomService;
        }

        public Invitation CreateInvitation(InvitationManagement parameters)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService();
            Invitation newInvitation = new Invitation();
            newInvitation.UserId = parameters.UserId;
            newInvitation.CourseId = parameters.CourseId;
            newInvitation.Role = CourseRoleEnum(parameters.Role);

            var request = classroomService.Invitations.Create(newInvitation);
            var response = request.Execute();

            return response;
        }

        private string CourseRoleEnum(int role)
        {
            throw new NotImplementedException();
        }
    }
}
