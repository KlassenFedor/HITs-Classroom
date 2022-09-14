using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HITs_classroom.Migrations
{
    /// <inheritdoc />
    public partial class CourseRelatedUserMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RelatedUser",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelatedUser",
                table: "Courses");
        }
    }
}
