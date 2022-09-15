using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HITs_classroom.Migrations
{
    /// <inheritdoc />
    public partial class CourseForeignKeyMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelatedUser",
                table: "Courses");

            migrationBuilder.AddColumn<string>(
                name: "RelatedUserId",
                table: "Courses",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_RelatedUserId",
                table: "Courses",
                column: "RelatedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_AspNetUsers_RelatedUserId",
                table: "Courses",
                column: "RelatedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_AspNetUsers_RelatedUserId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_RelatedUserId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "RelatedUserId",
                table: "Courses");

            migrationBuilder.AddColumn<string>(
                name: "RelatedUser",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
