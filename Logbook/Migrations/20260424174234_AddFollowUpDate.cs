using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logbook.Migrations
{
    /// <inheritdoc />
    public partial class AddFollowUpDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FollowUpDate",
                table: "JobApplications",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FollowUpDate",
                table: "JobApplications");
        }
    }
}
