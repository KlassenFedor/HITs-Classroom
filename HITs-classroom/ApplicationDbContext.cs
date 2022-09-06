using HITs_classroom.Models.Course;
using HITs_classroom.Models.Invitation;
using Microsoft.EntityFrameworkCore;

namespace HITs_classroom
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<InvitationDbModel> Invitations { get; set; }
        public DbSet<CourseDbModel> CourseDbModels { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InvitationDbModel>()
                .HasOne(i => i.Course)
                .WithMany(c => c.Invitations)
                .HasForeignKey(i => i.CourseId)
                .HasPrincipalKey(c => c.Id);
            modelBuilder.Entity<InvitationDbModel>()
                .HasKey(i => i.Id);
            modelBuilder.Entity<CourseDbModel>()
                .HasKey(x => x.Id);
        }
    }

}
