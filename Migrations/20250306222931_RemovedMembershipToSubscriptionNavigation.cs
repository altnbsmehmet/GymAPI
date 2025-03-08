using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class RemovedMembershipToSubscriptionNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subscription_MembershipId",
                table: "Subscription");

            migrationBuilder.CreateIndex(
                name: "IX_Subscription_MembershipId",
                table: "Subscription",
                column: "MembershipId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subscription_MembershipId",
                table: "Subscription");

            migrationBuilder.CreateIndex(
                name: "IX_Subscription_MembershipId",
                table: "Subscription",
                column: "MembershipId",
                unique: true);
        }
    }
}
