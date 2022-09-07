using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HITs_classroom.Migrations
{
    /// <inheritdoc />
    public partial class NewCourseModelForDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_CourseDbModels_CourseId",
                table: "Invitations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseDbModels",
                table: "CourseDbModels");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "CourseDbModels");

            migrationBuilder.RenameTable(
                name: "CourseDbModels",
                newName: "Courses");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Courses",
                table: "Courses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Invitations_Courses_CourseId",
                table: "Invitations",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_Courses_CourseId",
                table: "Invitations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Courses",
                table: "Courses");

            migrationBuilder.RenameTable(
                name: "Courses",
                newName: "CourseDbModels");

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "CourseDbModels",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseDbModels",
                table: "CourseDbModels",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Invitations_CourseDbModels_CourseId",
                table: "Invitations",
                column: "CourseId",
                principalTable: "CourseDbModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
