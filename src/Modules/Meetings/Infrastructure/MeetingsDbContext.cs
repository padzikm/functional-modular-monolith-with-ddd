using CompanyName.MyMeetings.Modules.Meetings.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.MyMeetings.Modules.Meetings.Infrastructure
{
    public class MeetingsDbContext : DbContext
    {
        public DbSet<Member> Members { get; set; }
        
        public DbSet<MeetingGroupProposal> MeetingGroupProposals { get; set; }

        public MeetingsDbContext(DbContextOptions options) : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // optionsBuilder.UseSqlServer("Server=localhost;Database=MyMeetings;User Id=sa;Password=SqlServer2019;");
        }
    }
}