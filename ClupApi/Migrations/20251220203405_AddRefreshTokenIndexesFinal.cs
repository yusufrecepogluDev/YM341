using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClupApi.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenIndexesFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_ExpiresAt",
                table: "RefreshToken",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_IsRevoked",
                table: "RefreshToken",
                column: "IsRevoked");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_Token",
                table: "RefreshToken",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_UserId_UserType",
                table: "RefreshToken",
                columns: new[] { "UserId", "UserType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RefreshToken_ExpiresAt",
                table: "RefreshToken");

            migrationBuilder.DropIndex(
                name: "IX_RefreshToken_IsRevoked",
                table: "RefreshToken");

            migrationBuilder.DropIndex(
                name: "IX_RefreshToken_Token",
                table: "RefreshToken");

            migrationBuilder.DropIndex(
                name: "IX_RefreshToken_UserId_UserType",
                table: "RefreshToken");
        }
    }
}
