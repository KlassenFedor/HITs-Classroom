using HITs_classroom.Models.User;
using HITs_classroom.Models.Course;
using HITs_classroom.Models.Invitation;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HITs_classroom.Models.Grade;

namespace HITs_classroom
{
    public class ApplicationDbContext : IdentityDbContext<GoogleUser>
    {
        public DbSet<InvitationDbModel> Invitations { get; set; }
        public DbSet<CourseDbModel> Courses { get; set; }
        public DbSet<GoogleUser> GoogleUsers { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GoogleUser>()
                .HasIndex(ca => ca.Email)
                .IsUnique();
            modelBuilder.Entity<GoogleUser>()
                .Property(ca => ca.UserName)
                .IsRequired();
            modelBuilder.Entity<GoogleUser>()
                .Property(ca => ca.NormalizedUserName)
                .IsRequired();

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
