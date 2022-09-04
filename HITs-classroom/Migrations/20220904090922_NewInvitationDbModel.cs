using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HITs_classroom.Migrations
{
    /// <inheritdoc />
    public partial class NewInvitationDbModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Invitations",
                newName: "Email");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "Invitations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateTime",
                table: "Invitations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "UpdateTime",
                table: "Invitations");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Invitations",
                newName: "UserId");
        }
    }
}
