using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CompanyName.MyMeetings.Modules.Meetings.Infrastructure.Migrations
{
    public partial class cmdid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByCmdId",
                schema: "meetings",
                table: "MeetingGroupProposals",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByCmdId",
                schema: "meetings",
                table: "MeetingGroupProposals");
        }
    }
}
