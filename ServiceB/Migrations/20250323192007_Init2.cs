using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceB.Migrations
{
    /// <inheritdoc />
    public partial class Init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortCode",
                table: "ModelA1s");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShortCode",
                table: "ModelA1s",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
