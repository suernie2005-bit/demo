using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Loai",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ten = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Mota = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loai", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SanPham",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenSP = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MoTaSP = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Gia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    LoaiId = table.Column<int>(type: "int", nullable: false),
                    Ngay = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPham", x => x.Id);
                    table.CheckConstraint("CK_SanPham_Gia_Positive", "[Gia] > 0");
                    table.CheckConstraint("CK_SanPham_SoLuong_NonNegative", "[SoLuong] >= 0");
                    table.ForeignKey(
                        name: "FK_SanPham_Loai",
                        column: x => x.LoaiId,
                        principalTable: "Loai",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Loai",
                columns: new[] { "Id", "Mota", "Ten" },
                values: new object[,]
                {
                    { 1, "Thiết bị và phụ kiện điện tử", "Điện tử" },
                    { 2, "Sản phẩm sử dụng trong gia đình", "Gia dụng" },
                    { 3, "Dụng cụ học tập và văn phòng", "Văn phòng phẩm" }
                });

            migrationBuilder.InsertData(
                table: "SanPham",
                columns: new[] { "Id", "Gia", "LoaiId", "MoTaSP", "Ngay", "SoLuong", "TenSP" },
                values: new object[,]
                {
                    { 1, 590000m, 1, "Tai nghe không dây mẫu", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 20, "Tai nghe Bluetooth" },
                    { 2, 350000m, 2, "Ấm đun nước mẫu", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 15, "Ấm siêu tốc" },
                    { 3, 45000m, 3, "Sổ ghi chép khổ A5", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 100, "Sổ ghi chép" }
                });

            migrationBuilder.CreateIndex(
                name: "UX_Loai_Ten",
                table: "Loai",
                column: "Ten",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_SanPham_LoaiId_TenSP",
                table: "SanPham",
                columns: new[] { "LoaiId", "TenSP" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SanPham");

            migrationBuilder.DropTable(
                name: "Loai");
        }
    }
}
