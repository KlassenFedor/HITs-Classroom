using HITs_classroom.Models.Teacher;
using Microsoft.EntityFrameworkCore;

namespace HITs_classroom.Services
{
    public interface ITeachersSearchservice
    {
        Task<List<TeacherInfoModel>> SearchTeachers(string namePart);
    }
    public class TeachersSearchServcie: ITeachersSearchservice
    {
        private ApplicationDbContext _context;
        public TeachersSearchServcie(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<TeacherInfoModel>> SearchTeachers(string namePart)
        {
            List<Teacher> suitableTeachers;
            List<TeacherInfoModel> teachersInfo;
            if (namePart == "")
            {
                suitableTeachers = await _context.Teachers.ToListAsync();
            }
            else
            {
                suitableTeachers = await _context.Teachers
                .Where(t => t.Name.ToLower().Contains(namePart.ToLower())).ToListAsync();
            }
            teachersInfo = suitableTeachers.Select(t => new TeacherInfoModel
            {
                Name = t.Name,
                Email = t.Email
            }).ToList();
            return teachersInfo;
        }
    }
}
