using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtGallery.Migrations
{
    /// <inheritdoc />
    public partial class AddHiddenFieldsToGiaoDich : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TrangThai",
                table: "giao_dich",
                newName: "trang_thai");

            migrationBuilder.RenameColumn(
                name: "PhuongThucThanhToan",
                table: "giao_dich",
                newName: "phuong_thuc_thanh_toan");

            migrationBuilder.AlterColumn<string>(
                name: "trang_thai",
                table: "giao_dich",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "phuong_thuc_thanh_toan",
                table: "giao_dich",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "is_hidden_by_buyer",
                table: "giao_dich",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_hidden_by_seller",
                table: "giao_dich",
                type: "bit",
                nullable: true,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_hidden_by_buyer",
                table: "giao_dich");

            migrationBuilder.DropColumn(
                name: "is_hidden_by_seller",
                table: "giao_dich");

            migrationBuilder.RenameColumn(
                name: "trang_thai",
                table: "giao_dich",
                newName: "TrangThai");

            migrationBuilder.RenameColumn(
                name: "phuong_thuc_thanh_toan",
                table: "giao_dich",
                newName: "PhuongThucThanhToan");

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "giao_dich",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "PhuongThucThanhToan",
                table: "giao_dich",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
