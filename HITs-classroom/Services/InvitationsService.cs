using Google;
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using HITs_classroom.Enums;
using HITs_classroom.Models.Course;
using HITs_classroom.Models.Invitation;
using HITs_classroom.Models.User;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace HITs_classroom.Services
{
    public interface IInvitationsService
    {
        Task<Invitation> CreateInvitation(InvitationCreatingModel parameters, string relatedUser);
        Task DeleteInvitation(string id, string relatedUser);
        Task<InvitationInfoModel> GetInvitation(string id, string relatedUser);
        Task<string> CheckInvitationStatus(string id, string relatedUser);
        Task UpdateCourseInvitations(string? courseId, string relatedUser);
        Task UpdateAllInvitations(string relatedUser);
        Task<List<InvitationInfoModel>> GetCourseInvitations(string courseId, string relatedUser);
        Task ResendInvitation(string invitationId, string relatedUser);
        Task<bool> CheckIfAllTeachersAcceptedInvitations(string courseId, string relatedUser);
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

        public async Task<Invitation> CreateInvitation(InvitationCreatingModel parameters, string relatedUser)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService(relatedUser);
            Invitation newInvitation = new Invitation();
            newInvitation.UserId = parameters.Email;
            newInvitation.CourseId = parameters.CourseId;
            newInvitation.Role = ((CourseRolesEnum)parameters.Role).ToString();

            var request = classroomService.Invitations.Create(newInvitation);
            var response = await request.ExecuteAsync();

            InvitationDbModel invitationDbModel = new InvitationDbModel();
            invitationDbModel.Id = response.Id;
            invitationDbModel.CourseId = parameters.CourseId;
            invitationDbModel.Email = parameters.Email;
            invitationDbModel.Role = parameters.Role;
            invitationDbModel.CreationTime = DateTime.Now;
            invitationDbModel.UpdateTime = DateTime.Now;
            await _context.Invitations.AddAsync(invitationDbModel);
            await _context.SaveChangesAsync();

            await CheckIfAllTeachersAcceptedInvitations(parameters.CourseId, relatedUser);

            return response;
        }

        public async Task DeleteInvitation(string id, string relatedUser)
        {
            var invitation = await _context.Invitations.Where(i => i.Id == id).FirstOrDefaultAsync();
            if (invitation == null)
            {
                throw new NullReferenceException();
            }
            InvitationDbModel? invitationDbModel = await _context.Invitations.FirstOrDefaultAsync(i => i.Id == id);
            if (invitationDbModel != null)
            {
                try
                {
                    ClassroomService classroomService = _googleClassroomService.GetClassroomService(relatedUser);
                    var request = classroomService.Invitations.Delete(id);
                    var response = await request.ExecuteAsync();
                }
                catch (GoogleApiException e)
                {
                    if (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        _context.Invitations.Remove(invitationDbModel);
                        await _context.SaveChangesAsync();

                        await CheckIfAllTeachersAcceptedInvitations(invitationDbModel.CourseId, relatedUser);
                        return;
                    }
                    else
                    {
                        throw;
                    }
                }
                _context.Invitations.Remove(invitationDbModel);
                await _context.SaveChangesAsync();

                await CheckIfAllTeachersAcceptedInvitations(invitationDbModel.CourseId, relatedUser);
            }
        }

        public async Task<InvitationInfoModel> GetInvitation(string id, string relatedUser)
        {
            var invitation = await _context.Invitations.FirstOrDefaultAsync(i => i.Id == id);
            if (invitation == null)
            {
                throw new NullReferenceException();
            }
            InvitationInfoModel response = new InvitationInfoModel();
            response.Id = invitation.Id;
            response.Email = invitation.Email;
            response.CourseId = invitation.CourseId;
            response.Role = ((CourseRolesEnum)invitation.Role).ToString();
            response.IsAccepted = response.IsAccepted;
            response.UpdateTime = response.UpdateTime;
            return response;
        }

        public async Task<string> CheckInvitationStatus(string id, string relatedUser)
        {
            var invitation = await _context.Invitations.Where(i => i.Id == id).FirstOrDefaultAsync();
            if (invitation != null)
            {
                if (invitation.IsAccepted)
                {
                    return InvitationStatusEnum.ACCEPTED.ToString();
                }
                else
                {
                    return InvitationStatusEnum.NOT_ACCEPTED.ToString();
                }
                
            }
            return InvitationStatusEnum.NOT_EXISTS.ToString();
        }

        public async Task UpdateAllInvitations(string relatedUser)
        {
            List<string> courses = new List<string>();
            courses.AddRange(await _context.Courses.Select(c => c.Id).ToListAsync());

            foreach (var course in courses)
            {
                await UpdateCourseInvitations(course, relatedUser);
            }
        }

        public async Task UpdateCourseInvitations(string courseId, string relatedUser)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService(relatedUser);

            var invitations = await _context.Invitations.Where(i => i.CourseId == courseId && !i.IsAccepted).ToListAsync();
            List<string> users = new List<string>();
            string pageToken = null;
            do
            {
                var request = classroomService.Courses.Teachers.List(courseId);
                request.PageSize = 100;
                request.PageToken = pageToken;
                var response = await request.ExecuteAsync();
                
                if (response.Teachers != null)
                {
                    users.AddRange(response.Teachers.Select(s => s.Profile.EmailAddress));
                }
                pageToken = response.NextPageToken;
            } while (pageToken != null);

            foreach (var invitation in invitations)
            {
                if (users.Contains(invitation.Email))
                {
                    invitation.IsAccepted = true;
                }
                invitation.UpdateTime = DateTime.Now;
                _context.Entry(invitation).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> CheckIfAllTeachersAcceptedInvitations(string courseId, string relatedUser) 
        {
            InvitationDbModel? invitation = await _context.Invitations.Where(i => i.CourseId == courseId &&
                i.Role == (int)CourseRolesEnum.TEACHER && !i.IsAccepted).FirstOrDefaultAsync();

            CourseDbModel? course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (course != null)
            {
                if (invitation == null)
                {
                    course.HasAllTeachers = true;
                }
                else
                {
                    course.HasAllTeachers = false;
                }
                _context.Entry(course).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return course.HasAllTeachers;
            }
            else
            {
                throw new NullReferenceException();
            }
        }

        public async Task<List<InvitationInfoModel>> GetCourseInvitations(string courseId, string relatedUser)
        {
            List<InvitationDbModel> invitations = await _context.Invitations.Where(i => i.CourseId == courseId).ToListAsync();
            List<InvitationInfoModel> invitationInfoModels = invitations.Select(i => new InvitationInfoModel
            {
                Id = i.Id,
                CourseId = i.CourseId,
                Email = i.Email,
                Role = ((CourseRolesEnum)i.Role).ToString(),
                IsAccepted = i.IsAccepted,
                UpdateTime = i.UpdateTime
            }).ToList();

            return invitationInfoModels;
        }

        public async Task ResendInvitation(string invitationId, string relatedUser)
        {
            var invitation = await _context.Invitations.Where(i => i.Id == invitationId).FirstOrDefaultAsync();
            if (invitation == null)
            {
                throw new NullReferenceException();
            }
            InvitationDbModel? oldInvitation = await _context.Invitations.FindAsync(invitationId);
            await DeleteInvitation(invitationId, relatedUser);
            if (!(oldInvitation == null))
            {
                InvitationCreatingModel newInvitation = new InvitationCreatingModel();
                newInvitation.CourseId = oldInvitation.CourseId;
                newInvitation.Email = oldInvitation.Email;
                newInvitation.Role = oldInvitation.Role;
                await CreateInvitation(newInvitation, relatedUser);
            }
            else
            {
                throw new NullReferenceException();
            }
        }
    }
}
