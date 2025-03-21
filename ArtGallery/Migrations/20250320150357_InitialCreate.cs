using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtGallery.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nguoi_dung",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenNguoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GioiTinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgaySinh = table.Column<DateOnly>(type: "date", nullable: true),
                    BaiHat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnhDaiDien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoverImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HienThiGioiTinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HienThiDiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HienThiNgaySinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HienThiNamSinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nguoi_dung", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "the_loai",
                columns: table => new
                {
                    ma_the_loai = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ten_the_loai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__the_loai__489AA0F38C7B2D0E", x => x.ma_the_loai);
                });

            migrationBuilder.CreateTable(
                name: "the_tag",
                columns: table => new
                {
                    ma_tag = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ten_tag = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__the_tag__099A6217E57B8670", x => x.ma_tag);
                });

            migrationBuilder.CreateTable(
                name: "doanh_thu",
                columns: table => new
                {
                    ma_nguoi_dung = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    tong_doanh_thu = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0m),
                    so_tranh_ban_duoc = table.Column<int>(type: "int", nullable: true, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__doanh_th__19C32CF7AA9DECC6", x => x.ma_nguoi_dung);
                    table.ForeignKey(
                        name: "FK__doanh_thu__ma_ng__290D0E62",
                        column: x => x.ma_nguoi_dung,
                        principalTable: "nguoi_dung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "media",
                columns: table => new
                {
                    ma_media = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_nguoi_dung = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    loai_media = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    duong_dan = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__media__6CED7F1DFD307904", x => x.ma_media);
                    table.ForeignKey(
                        name: "FK__media__ma_nguoi___2CDD9F46",
                        column: x => x.ma_nguoi_dung,
                        principalTable: "nguoi_dung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "theo_doi",
                columns: table => new
                {
                    ma_nguoi_theo_doi = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ma_nguoi_duoc_theo_doi = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ngay_theo_doi = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__theo_doi__E8A6156EE67C0BC9", x => new { x.ma_nguoi_theo_doi, x.ma_nguoi_duoc_theo_doi });
                    table.ForeignKey(
                        name: "FK__theo_doi__ma_ngu__17E28260",
                        column: x => x.ma_nguoi_theo_doi,
                        principalTable: "nguoi_dung",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__theo_doi__ma_ngu__18D6A699",
                        column: x => x.ma_nguoi_duoc_theo_doi,
                        principalTable: "nguoi_dung",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tranh",
                columns: table => new
                {
                    ma_tranh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_nguoi_dung = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    tieu_de = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    duong_dan_anh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ngay_dang = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    gia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Đang bán"),
                    so_luong_ton = table.Column<int>(type: "int", nullable: false),
                    da_ban = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__tranh__633AC9ECC367B622", x => x.ma_tranh);
                    table.ForeignKey(
                        name: "FK__tranh__ma_nguoi___7C3A67EB",
                        column: x => x.ma_nguoi_dung,
                        principalTable: "nguoi_dung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_claims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_claims_nguoi_dung_UserId",
                        column: x => x.UserId,
                        principalTable: "nguoi_dung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_logins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_logins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_user_logins_nguoi_dung_UserId",
                        column: x => x.UserId,
                        principalTable: "nguoi_dung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_tokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_tokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_user_tokens_nguoi_dung_UserId",
                        column: x => x.UserId,
                        principalTable: "nguoi_dung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_claims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_role_claims_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_user_roles_nguoi_dung_UserId",
                        column: x => x.UserId,
                        principalTable: "nguoi_dung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "binh_luan",
                columns: table => new
                {
                    ma_binh_luan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_tranh = table.Column<int>(type: "int", nullable: false),
                    ma_nguoi_dung = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    noi_dung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ngay_binh_luan = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__binh_lua__300DD2D8D7F67231", x => x.ma_binh_luan);
                    table.ForeignKey(
                        name: "FK__binh_luan__ma_ng__0E591826",
                        column: x => x.ma_nguoi_dung,
                        principalTable: "nguoi_dung",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__binh_luan__ma_tr__0D64F3ED",
                        column: x => x.ma_tranh,
                        principalTable: "tranh",
                        principalColumn: "ma_tranh",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "giao_dich",
                columns: table => new
                {
                    ma_giao_dich = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_nguoi_mua = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ma_tranh = table.Column<int>(type: "int", nullable: false),
                    so_luong = table.Column<int>(type: "int", nullable: false),
                    so_tien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ngay_mua = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__giao_dic__FB80ED3254B2912A", x => x.ma_giao_dich);
                    table.ForeignKey(
                        name: "FK__giao_dich__ma_ng__2354350C",
                        column: x => x.ma_nguoi_mua,
                        principalTable: "nguoi_dung",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__giao_dich__ma_tr__24485945",
                        column: x => x.ma_tranh,
                        principalTable: "tranh",
                        principalColumn: "ma_tranh");
                });

            migrationBuilder.CreateTable(
                name: "luot_thich",
                columns: table => new
                {
                    ma_luot_thich = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_tranh = table.Column<int>(type: "int", nullable: false),
                    ma_nguoi_dung = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ngay_thich = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__luot_thi__64AF50517DA222E2", x => x.ma_luot_thich);
                    table.ForeignKey(
                        name: "FK__luot_thic__ma_ng__1411F17C",
                        column: x => x.ma_nguoi_dung,
                        principalTable: "nguoi_dung",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__luot_thic__ma_tr__131DCD43",
                        column: x => x.ma_tranh,
                        principalTable: "tranh",
                        principalColumn: "ma_tranh",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "luu_tranh",
                columns: table => new
                {
                    ma_luu_tranh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_nguoi_dung = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ma_tranh = table.Column<int>(type: "int", nullable: false),
                    ngay_luu = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__luu_tran__0893EFAE9404701B", x => x.ma_luu_tranh);
                    table.ForeignKey(
                        name: "FK__luu_tranh__ma_ng__1D9B5BB6",
                        column: x => x.ma_nguoi_dung,
                        principalTable: "nguoi_dung",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__luu_tranh__ma_tr__1E8F7FEF",
                        column: x => x.ma_tranh,
                        principalTable: "tranh",
                        principalColumn: "ma_tranh",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "noi_bat",
                columns: table => new
                {
                    ma_nguoi_dung = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ma_tranh = table.Column<int>(type: "int", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__noi_bat__CFF08069508BECFF", x => new { x.ma_nguoi_dung, x.ma_tranh });
                    table.ForeignKey(
                        name: "FK__noi_bat__ma_nguo__31A25463",
                        column: x => x.ma_nguoi_dung,
                        principalTable: "nguoi_dung",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__noi_bat__ma_tran__3296789C",
                        column: x => x.ma_tranh,
                        principalTable: "tranh",
                        principalColumn: "ma_tranh",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tranh_the_loai",
                columns: table => new
                {
                    ma_tranh = table.Column<int>(type: "int", nullable: false),
                    ma_the_loai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__tranh_th__47B363E3D8A01C76", x => new { x.ma_tranh, x.ma_the_loai });
                    table.ForeignKey(
                        name: "FK__tranh_the__ma_th__02E7657A",
                        column: x => x.ma_the_loai,
                        principalTable: "the_loai",
                        principalColumn: "ma_the_loai",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__tranh_the__ma_tr__01F34141",
                        column: x => x.ma_tranh,
                        principalTable: "tranh",
                        principalColumn: "ma_tranh",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tranh_the_tag",
                columns: table => new
                {
                    ma_tranh = table.Column<int>(type: "int", nullable: false),
                    ma_tag = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__tranh_th__03A36FCDE94B1BA4", x => new { x.ma_tranh, x.ma_tag });
                    table.ForeignKey(
                        name: "FK__tranh_the__ma_ta__09946309",
                        column: x => x.ma_tag,
                        principalTable: "the_tag",
                        principalColumn: "ma_tag",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__tranh_the__ma_tr__08A03ED0",
                        column: x => x.ma_tranh,
                        principalTable: "tranh",
                        principalColumn: "ma_tranh",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_binh_luan_ma_nguoi_dung",
                table: "binh_luan",
                column: "ma_nguoi_dung");

            migrationBuilder.CreateIndex(
                name: "IX_binh_luan_ma_tranh",
                table: "binh_luan",
                column: "ma_tranh");

            migrationBuilder.CreateIndex(
                name: "IX_giao_dich_ma_nguoi_mua",
                table: "giao_dich",
                column: "ma_nguoi_mua");

            migrationBuilder.CreateIndex(
                name: "IX_giao_dich_ma_tranh",
                table: "giao_dich",
                column: "ma_tranh");

            migrationBuilder.CreateIndex(
                name: "IX_luot_thich_ma_nguoi_dung",
                table: "luot_thich",
                column: "ma_nguoi_dung");

            migrationBuilder.CreateIndex(
                name: "unique_like",
                table: "luot_thich",
                columns: new[] { "ma_tranh", "ma_nguoi_dung" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_luu_tranh_ma_tranh",
                table: "luu_tranh",
                column: "ma_tranh");

            migrationBuilder.CreateIndex(
                name: "unique_bookmark",
                table: "luu_tranh",
                columns: new[] { "ma_nguoi_dung", "ma_tranh" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_media_ma_nguoi_dung",
                table: "media",
                column: "ma_nguoi_dung");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "nguoi_dung",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "nguoi_dung",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_noi_bat_ma_tranh",
                table: "noi_bat",
                column: "ma_tranh");

            migrationBuilder.CreateIndex(
                name: "IX_role_claims_RoleId",
                table: "role_claims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "roles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__the_loai__87296EA3BFC72A9B",
                table: "the_loai",
                column: "ten_the_loai",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__the_tag__56DB79DEC5AA1248",
                table: "the_tag",
                column: "ten_tag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_theo_doi_ma_nguoi_duoc_theo_doi",
                table: "theo_doi",
                column: "ma_nguoi_duoc_theo_doi");

            migrationBuilder.CreateIndex(
                name: "IX_tranh_ma_nguoi_dung",
                table: "tranh",
                column: "ma_nguoi_dung");

            migrationBuilder.CreateIndex(
                name: "IX_tranh_the_loai_ma_the_loai",
                table: "tranh_the_loai",
                column: "ma_the_loai");

            migrationBuilder.CreateIndex(
                name: "IX_tranh_the_tag_ma_tag",
                table: "tranh_the_tag",
                column: "ma_tag");

            migrationBuilder.CreateIndex(
                name: "IX_user_claims_UserId",
                table: "user_claims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_logins_UserId",
                table: "user_logins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_RoleId",
                table: "user_roles",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "binh_luan");

            migrationBuilder.DropTable(
                name: "doanh_thu");

            migrationBuilder.DropTable(
                name: "giao_dich");

            migrationBuilder.DropTable(
                name: "luot_thich");

            migrationBuilder.DropTable(
                name: "luu_tranh");

            migrationBuilder.DropTable(
                name: "media");

            migrationBuilder.DropTable(
                name: "noi_bat");

            migrationBuilder.DropTable(
                name: "role_claims");

            migrationBuilder.DropTable(
                name: "theo_doi");

            migrationBuilder.DropTable(
                name: "tranh_the_loai");

            migrationBuilder.DropTable(
                name: "tranh_the_tag");

            migrationBuilder.DropTable(
                name: "user_claims");

            migrationBuilder.DropTable(
                name: "user_logins");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "user_tokens");

            migrationBuilder.DropTable(
                name: "the_loai");

            migrationBuilder.DropTable(
                name: "the_tag");

            migrationBuilder.DropTable(
                name: "tranh");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "nguoi_dung");
        }
    }
}
