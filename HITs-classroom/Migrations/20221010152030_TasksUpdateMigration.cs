using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HITs_classroom.Migrations
{
    public partial class TasksUpdateMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCoompleted",
                table: "Tasks");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Tasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsCreated",
                table: "PreCreatedCourses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RealCourseId",
                table: "PreCreatedCourses",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreCreatedCourses_RealCourseId",
                table: "PreCreatedCourses",
                column: "RealCourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_PreCreatedCourses_Courses_RealCourseId",
                table: "PreCreatedCourses",
                column: "RealCourseId",
                principalTable: "Courses",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PreCreatedCourses_Courses_RealCourseId",
                table: "PreCreatedCourses");

            migrationBuilder.DropIndex(
                name: "IX_PreCreatedCourses_RealCourseId",
                table: "PreCreatedCourses");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "IsCreated",
                table: "PreCreatedCourses");

            migrationBuilder.DropColumn(
                name: "RealCourseId",
                table: "PreCreatedCourses");

            migrationBuilder.AddColumn<bool>(
                name: "IsCoompleted",
                table: "Tasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
