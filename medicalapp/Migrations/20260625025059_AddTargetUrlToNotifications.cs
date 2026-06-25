using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace medicalapp.Migrations
{
    /// <inheritdoc />
    public partial class AddTargetUrlToNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TargetUrl",
                table: "Notifications",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetUrl",
                table: "Notifications");
        }
    }
}
