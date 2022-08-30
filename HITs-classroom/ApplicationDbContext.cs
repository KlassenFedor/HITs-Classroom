using HITs_classroom.Models.Invitation;
using Microsoft.EntityFrameworkCore;

namespace HITs_classroom
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<InvitationDbModel> Invitations { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InvitationDbModel>().HasKey(x => x.Id);
        }
    }

}
