using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CompanyName.MyMeetings.Modules.Meetings.Infrastructure.Migrations
{
    public partial class MeetingGroupProposal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MeetingGroupProposals",
                schema: "meetings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProposalMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProposalDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LocationCity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocationCountryCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingGroupProposals", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeetingGroupProposals",
                schema: "meetings");
        }
    }
}
