using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostaKutusuServisi.Migrations
{
    /// <inheritdoc />
    public partial class mig_soft_delete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeletedByReceiver",
                table: "UserMessages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeletedBySender",
                table: "UserMessages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeletedByReceiver",
                table: "UserMessages");

            migrationBuilder.DropColumn(
                name: "IsDeletedBySender",
                table: "UserMessages");
        }
    }
}
