using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CateringWebsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserLocationAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LocationAddress",
                table: "AspNetUsers",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocationAddress",
                table: "AspNetUsers");
        }
    }
}
