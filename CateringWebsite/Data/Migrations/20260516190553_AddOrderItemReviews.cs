using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CateringWebsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderItemReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderItemReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderItemId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    CaretakerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MenuRating = table.Column<int>(type: "int", nullable: false),
                    CaretakerRating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(800)", maxLength: 800, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItemReviews_AspNetUsers_CaretakerId",
                        column: x => x.CaretakerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItemReviews_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItemReviews_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItemReviews_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemReviews_CaretakerId",
                table: "OrderItemReviews",
                column: "CaretakerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemReviews_MenuItemId",
                table: "OrderItemReviews",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemReviews_OrderItemId",
                table: "OrderItemReviews",
                column: "OrderItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemReviews_UserId",
                table: "OrderItemReviews",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItemReviews");
        }
    }
}
