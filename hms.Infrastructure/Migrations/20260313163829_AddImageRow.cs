using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImageRow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Hotels",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImgPublicId",
                table: "Hotels",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Hotels");

            migrationBuilder.DropColumn(
                name: "ImgPublicId",
                table: "Hotels");
        }
    }
}
