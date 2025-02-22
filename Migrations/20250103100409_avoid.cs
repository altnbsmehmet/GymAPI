using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class avoid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SubscriptionId",
                table: "Subscription",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "MembershipId",
                table: "Membership",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "MemberId",
                table: "Member",
                newName: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Subscription",
                newName: "SubscriptionId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Membership",
                newName: "MembershipId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Member",
                newName: "MemberId");
        }
    }
}
