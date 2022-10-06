using Google;
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using HITs_classroom.Enums;
using HITs_classroom.Models.Course;
using HITs_classroom.Models.Invitation;
using Microsoft.EntityFrameworkCore;

namespace HITs_classroom.Services
{
    public interface IInvitationsService
    {
        Task<Invitation> CreateInvitation(InvitationCreatingModel parameters);
        Task DeleteInvitation(string id);
        Task<InvitationInfoModel> GetInvitation(string id);
        Task<string> CheckInvitationStatus(string id);
        Task UpdateCourseInvitations(string? courseId);
        Task UpdateAllInvitations();
        Task<List<InvitationInfoModel>> GetCourseInvitations(string courseId);
        Task ResendInvitation(string invitationId);
        Task<bool> CheckIfAllTeachersAcceptedInvitations(string courseId);
    }

    public class InvitationsService: IInvitationsService
    {
        private ApplicationDbContext _context;
        private ClassroomService _service;
        public InvitationsService(ApplicationDbContext context,
            GoogleClassroomServiceForServiceAccount googleClassroomServiceForServiceAccount)
        {
            _context = context;
            _service = googleClassroomServiceForServiceAccount.GetClassroomService();
        }

        public async Task<Invitation> CreateInvitation(InvitationCreatingModel parameters)
        {
            Invitation newInvitation = new Invitation();
            newInvitation.UserId = parameters.Email;
            newInvitation.CourseId = parameters.CourseId;
            newInvitation.Role = ((CourseRolesEnum)parameters.Role).ToString();

            var request = _service.Invitations.Create(newInvitation);
            var response = await request.ExecuteAsync();

            InvitationDbModel invitationDbModel = new InvitationDbModel();
            invitationDbModel.Id = response.Id;
            invitationDbModel.CourseId = parameters.CourseId;
            invitationDbModel.Email = parameters.Email;
            invitationDbModel.Role = parameters.Role;
            invitationDbModel.CreationTime = DateTimeOffset.Now.ToUniversalTime();
            invitationDbModel.UpdateTime = DateTimeOffset.Now.ToUniversalTime();
            await _context.Invitations.AddAsync(invitationDbModel);
            await _context.SaveChangesAsync();

            await CheckIfAllTeachersAcceptedInvitations(parameters.CourseId);

            return response;
        }

        public async Task DeleteInvitation(string id)
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
                    var request = _service.Invitations.Delete(id);
                    var response = await request.ExecuteAsync();
                }
                catch (GoogleApiException e)
                {
                    if (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        _context.Invitations.Remove(invitationDbModel);
                        await _context.SaveChangesAsync();

                        await CheckIfAllTeachersAcceptedInvitations(invitationDbModel.CourseId);
                        return;
                    }
                    else
                    {
                        throw;
                    }
                }
                _context.Invitations.Remove(invitationDbModel);
                await _context.SaveChangesAsync();

                await CheckIfAllTeachersAcceptedInvitations(invitationDbModel.CourseId);
            }
        }

        public async Task<InvitationInfoModel> GetInvitation(string id)
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

        public async Task<string> CheckInvitationStatus(string id)
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

        public async Task UpdateAllInvitations()
        {
            List<string> courses = new List<string>();
            courses.AddRange(await _context.Courses.Select(c => c.Id).ToListAsync());

            foreach (var course in courses)
            {
                await UpdateCourseInvitations(course);
            }
        }

        public async Task UpdateCourseInvitations(string courseId)
        {
            var invitations = await _context.Invitations.Where(i => i.CourseId == courseId && !i.IsAccepted).ToListAsync();
            List<string> users = new List<string>();
            string pageToken = null;
            do
            {
                var request = _service.Courses.Teachers.List(courseId);
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
                invitation.UpdateTime = DateTimeOffset.Now.ToUniversalTime();
                _context.Entry(invitation).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
        }

        //updates invitations for course

        public async Task UpdateCourseInvitationsNV(string courseId)
        {
            await UpdateCourseInvitationsNV(courseId);
            List<Invitation> invitations = new List<Invitation>();
            string pageToken = null;
            do
            {
                var request = _service.Invitations.List();
                request.CourseId = courseId;
                request.PageSize = 100;
                request.PageToken = pageToken;
                var response = await request.ExecuteAsync();

                if (response.Invitations != null)
                {
                    invitations.AddRange(response.Invitations);
                }
                pageToken = response.NextPageToken;
            } while (pageToken != null);

            var dbInvitations = await _context.Invitations
                        .Where(i => !invitations.Select(i => i.Id).Contains(i.Id) && i.CourseId == courseId).ToListAsync();
            foreach (var dbInvitation in dbInvitations)
            {
                dbInvitation.IsAccepted = true;
                dbInvitation.UpdateTime = DateTimeOffset.Now.ToUniversalTime();
            }
            await _context.SaveChangesAsync();
        }

        //check if invitations exists in google classroom
        public async Task<bool> CheckIfAllTeachersAcceptedInvitationsNV(string courseId)
        {
            await UpdateCourseInvitationsNV(courseId);
            List<Invitation> invitations = new List<Invitation>();
            var request = _service.Invitations.List();
            request.CourseId = courseId;
            request.PageSize = 1;
            var response = await request.ExecuteAsync();

            if (response.Invitations != null)
            {
                invitations.AddRange(response.Invitations);
            }

            return invitations.Count() > 0;
        }

        public async Task<bool> CheckIfAllTeachersAcceptedInvitations(string courseId)
        {
            try
            {
                await UpdateCourseInvitations(courseId);
            }
            catch
            {
                throw;
            }
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

        public async Task<List<InvitationInfoModel>> GetCourseInvitations(string courseId)
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

        public async Task ResendInvitation(string invitationId)
        {
            var invitation = await _context.Invitations.Where(i => i.Id == invitationId).FirstOrDefaultAsync();
            if (invitation == null)
            {
                throw new NullReferenceException();
            }
            InvitationDbModel? oldInvitation = await _context.Invitations.FindAsync(invitationId);
            await DeleteInvitation(invitationId);
            if (!(oldInvitation == null))
            {
                InvitationCreatingModel newInvitation = new InvitationCreatingModel();
                newInvitation.CourseId = oldInvitation.CourseId;
                newInvitation.Email = oldInvitation.Email;
                newInvitation.Role = oldInvitation.Role;
                await CreateInvitation(newInvitation);
            }
            else
            {
                throw new NullReferenceException();
            }
        }
    }
}
