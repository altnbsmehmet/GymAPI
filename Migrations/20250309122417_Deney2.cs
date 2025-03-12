using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class Deney2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscription_Membership_MembershipId1",
                table: "Subscription");

            migrationBuilder.DropIndex(
                name: "IX_Subscription_MembershipId1",
                table: "Subscription");

            migrationBuilder.DropColumn(
                name: "MembershipId1",
                table: "Subscription");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MembershipId1",
                table: "Subscription",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscription_MembershipId1",
                table: "Subscription",
                column: "MembershipId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscription_Membership_MembershipId1",
                table: "Subscription",
                column: "MembershipId1",
                principalTable: "Membership",
                principalColumn: "Id");
        }
    }
}
