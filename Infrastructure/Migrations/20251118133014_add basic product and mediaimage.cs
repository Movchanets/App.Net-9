using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addbasicproductandmediaimage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "DomainUsers");

            migrationBuilder.AddColumn<Guid>(
                name: "AvatarId",
                table: "DomainUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MediaImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageKey = table.Column<string>(type: "text", nullable: false),
                    MimeType = table.Column<string>(type: "text", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    AltText = table.Column<string>(type: "text", nullable: true),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaImages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DomainUsers_AvatarId",
                table: "DomainUsers",
                column: "AvatarId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaImages_ProductId",
                table: "MediaImages",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaImages_StorageKey",
                table: "MediaImages",
                column: "StorageKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Sku",
                table: "Products",
                column: "Sku",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DomainUsers_MediaImages_AvatarId",
                table: "DomainUsers",
                column: "AvatarId",
                principalTable: "MediaImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DomainUsers_MediaImages_AvatarId",
                table: "DomainUsers");

            migrationBuilder.DropTable(
                name: "MediaImages");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropIndex(
                name: "IX_DomainUsers_AvatarId",
                table: "DomainUsers");

            migrationBuilder.DropColumn(
                name: "AvatarId",
                table: "DomainUsers");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "DomainUsers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
