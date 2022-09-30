using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HITs_classroom.Migrations
{
    /// <inheritdoc />
    public partial class DbForServiceAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassroomAdminCourseDbModel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClassroomAdminCourseDbModel",
                columns: table => new
                {
                    CoursesId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RelatedUsersId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassroomAdminCourseDbModel", x => new { x.CoursesId, x.RelatedUsersId });
                    table.ForeignKey(
                        name: "FK_ClassroomAdminCourseDbModel_AspNetUsers_RelatedUsersId",
                        column: x => x.RelatedUsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassroomAdminCourseDbModel_Courses_CoursesId",
                        column: x => x.CoursesId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassroomAdminCourseDbModel_RelatedUsersId",
                table: "ClassroomAdminCourseDbModel",
                column: "RelatedUsersId");
        }
    }
}
