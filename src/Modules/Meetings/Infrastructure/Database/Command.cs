using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CompanyName.MyMeetings.Modules.Meetings.Infrastructure.Database
{
    public enum CommandStatus 
    {
        Accepted,
        Rejected,
        Completed,
        Failed
    }
    
    public class Command
    {
        public Guid Id { get; set; }
        
        public DateTime DateTime { get; set; }
        
        public string Name { get; set; }
        
        public CommandStatus CommandStatus { get; set; }
        
        public string Error { get; set; }
    }
    
    internal class CommandConfiguration: IEntityTypeConfiguration<Command>
    {
        public void Configure(EntityTypeBuilder<Command> builder)
        {
            builder.ToTable("Commands", "meetings");
        }
    }
}