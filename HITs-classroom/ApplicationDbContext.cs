using HITs_classroom.Models.Course;
using HITs_classroom.Models.Invitation;
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
