using Azure;
using Google;
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using HITs_classroom.Enums;
using HITs_classroom.Models.Invitation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Diagnostics;
using System.Text.Json;

namespace HITs_classroom.Services
{
    public interface IInvitationsService
    {
        Task<Invitation> CreateInvitation(InvitationManagementModel parameters);
        Task<string> DeleteInvitation(string id);
        Task<Invitation> GetInvitation(string id);
        Task<string> CheckInvitationStatus(string id);
        Task UpdateCourseInvitations(string? courseId);
        Task UpdateAllInvitations();
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

        public async Task<Invitation> CreateInvitation(InvitationManagementModel parameters)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService();
            Invitation newInvitation = new Invitation();
            newInvitation.UserId = parameters.UserId;
            newInvitation.CourseId = parameters.CourseId;
            newInvitation.Role = ((CourseRoleEnum)parameters.Role).ToString();

            var request = classroomService.Invitations.Create(newInvitation);
            var response = request.Execute();

            InvitationDbModel invitationDbModel = new InvitationDbModel();
            invitationDbModel.Id = response.Id;
            invitationDbModel.CourseId = parameters.CourseId;
            invitationDbModel.UserId = parameters.UserId;
            invitationDbModel.Role = parameters.Role;
            await _context.Invitations.AddAsync(invitationDbModel);
            await _context.SaveChangesAsync();

            return response;
        }

        public async Task<string> DeleteInvitation(string id)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService();
            var request = classroomService.Invitations.Delete(id);
            var response = request.Execute();

            InvitationDbModel? invitationDbModel = await _context.Invitations.FirstOrDefaultAsync(i => i.Id == id);
            if (invitationDbModel != null)
            {
                _context.Invitations.Remove(invitationDbModel);
                await _context.SaveChangesAsync();
            }

            return JsonSerializer.Serialize(response);
        }

        public async Task<Invitation> GetInvitation(string id)
        {
            var invitation = await _context.Invitations.FirstOrDefaultAsync(i => i.Id == id);
            Invitation response = new Invitation();
            if (invitation != null)
            {
                response.Id = invitation.Id;
                response.UserId = invitation.UserId;
                response.CourseId = invitation.CourseId;
                response.Role = ((CourseRoleEnum)invitation.Role).ToString();
                return response;
            }

            return null;
        }

        public async Task<string> CheckInvitationStatus(string id)
        {
            var invitation = await _context.Invitations.Where(i => i.Id == id).FirstOrDefaultAsync();
            if (invitation != null)
            {
                if (invitation.IsAccepted)
                {
                    return InvitationStatus.ACCEPTED.ToString();
                }
                else
                {
                    return InvitationStatus.NOT_ACCEPTED.ToString();
                }
                
            }
            return InvitationStatus.NOT_EXISTS.ToString();
        }

        public async Task UpdateAllInvitations()
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService();
            List<string> courses = new List<string>();
            string pageToken = null;
            do
            {
                var request = classroomService.Courses.List();
                request.PageSize = 100;
                request.PageToken = pageToken;
                var response = request.Execute();
                if (response.Courses != null)
                {
                    courses.AddRange(response.Courses.Select(c => c.Id));
                }
                pageToken = response.NextPageToken;
            } while (pageToken != null);

            foreach (var course in courses)
            {
                await UpdateCourseInvitations(course);
            }
        }

        public async Task UpdateCourseInvitations(string courseId)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService();

            var invitations = await _context.Invitations.Where(i => i.CourseId == courseId && !i.IsAccepted).ToListAsync();
            List<string> users = new List<string>();
            string pageToken = null;
            do
            {
                var request = classroomService.Courses.Students.List(courseId);
                request.PageSize = 100;
                request.PageToken = pageToken;
                var response = request.Execute();
                
                if (response.Students != null)
                {
                    users.AddRange(response.Students.Select(s => s.Profile.EmailAddress));
                }
                pageToken = response.NextPageToken;
            } while (pageToken != null);

            foreach (var invitation in invitations)
            {
                if (users.Contains(invitation.UserId))
                {
                    invitation.IsAccepted = true;
                    _context.Entry(invitation).State = EntityState.Modified;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
