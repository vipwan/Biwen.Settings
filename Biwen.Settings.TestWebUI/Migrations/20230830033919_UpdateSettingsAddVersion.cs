using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biwen.Settings.TestWebUI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSettingsAddVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "Settings",
                type: "BLOB",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "Settings");
        }
    }
}
