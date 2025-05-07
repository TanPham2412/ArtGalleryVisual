using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtGallery.Migrations
{
    /// <inheritdoc />
    public partial class AddLockoutReasonColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LockoutReason",
                table: "nguoi_dung",
                newName: "lockout_reason");

            migrationBuilder.AlterColumn<string>(
                name: "lockout_reason",
                table: "nguoi_dung",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "lockout_reason",
                table: "nguoi_dung",
                newName: "LockoutReason");

            migrationBuilder.AlterColumn<string>(
                name: "LockoutReason",
                table: "nguoi_dung",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
