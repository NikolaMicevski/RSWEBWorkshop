using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkshopV2.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlToTeacherAndStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Teacher",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Student",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Teacher");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Student");
        }
    }
}
