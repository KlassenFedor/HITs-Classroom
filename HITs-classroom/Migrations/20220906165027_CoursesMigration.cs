using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HITs_classroom.Migrations
{
    /// <inheritdoc />
    public partial class CoursesMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "Invitations",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "CourseDbModels",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Section = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DescriptionHeading = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Room = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnerId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourseState = table.Column<int>(type: "int", nullable: false),
                    HasAllTeachers = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseDbModels", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_CourseId",
                table: "Invitations",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invitations_CourseDbModels_CourseId",
                table: "Invitations",
                column: "CourseId",
                principalTable: "CourseDbModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_CourseDbModels_CourseId",
                table: "Invitations");

            migrationBuilder.DropTable(
                name: "CourseDbModels");

            migrationBuilder.DropIndex(
                name: "IX_Invitations_CourseId",
                table: "Invitations");

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "Invitations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
