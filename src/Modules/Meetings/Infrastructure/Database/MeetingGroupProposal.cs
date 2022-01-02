using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CompanyName.MyMeetings.Modules.Meetings.Infrastructure.Database
{
    public enum MeetingGroupProposalStatus {
        InVerification,
        Accepted   
    }
    
    public class MeetingGroupProposal
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid ProposalMemberId { get; set; }
        public DateTime ProposalDate { get; set; }
        public string LocationCity { get; set; }
        public string LocationCountryCode { get; set; }
        public MeetingGroupProposalStatus Status { get; set; }
    }
    
    internal class MeetingGroupProposalConfiguration: IEntityTypeConfiguration<MeetingGroupProposal>
    {
        public void Configure(EntityTypeBuilder<MeetingGroupProposal> builder)
        {
            builder.ToTable("MeetingGroupProposals", "meetings");
        }
    }
}