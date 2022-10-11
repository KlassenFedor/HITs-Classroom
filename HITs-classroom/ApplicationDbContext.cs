using HITs_classroom.Models.Course;
using HITs_classroom.Models.CoursesList;
using HITs_classroom.Models.Invitation;
using HITs_classroom.Models.Task;
using HITs_classroom.Models.Teacher;
using HITs_classroom.Models.TsuAccount;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HITs_classroom
{
    public class ApplicationDbContext : IdentityDbContext<TsuAccountUser>
    {
        public DbSet<InvitationDbModel> Invitations { get; set; }
        public DbSet<CourseDbModel> Courses { get; set; }
        public DbSet<TsuAccountUser> TsuUsers { get; set; }
        public DbSet<CoursePreCreatingModel> PreCreatedCourses { get; set; }
        public DbSet<AssignedTask> Tasks { get; set; }
        public DbSet<Teacher> Teachers { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TsuAccountUser>()
                .ToTable("TsuUsers")
                .HasKey(u => u.Id);

            modelBuilder.Entity<IdentityUserToken<string>>()
                .ToTable("Tokens");

            modelBuilder.Entity<InvitationDbModel>(i => 
            { 
                i.HasOne(i => i.Course)
                .WithMany(c => c.Invitations)
                .HasForeignKey(i => i.CourseId)
                .HasPrincipalKey(c => c.Id);

                i.HasKey(i => i.Id); 
            });

            modelBuilder.Entity<CourseDbModel>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<CoursePreCreatingModel>()
                .HasOne(c => c.Task)
                .WithMany(t => t.RelatedCourses)
                .HasForeignKey(c => c.TaskId);
        }
    }
}
