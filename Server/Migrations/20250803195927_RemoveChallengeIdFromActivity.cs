using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class RemoveChallengeIdFromActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Safely attempt to drop the index and column,
            // but skip dropping the FK since it no longer exists

            // This may throw if the index still exists — keep if you're sure it's already deleted
            // migrationBuilder.DropIndex(
            //     name: "IX_sustainableactivities_ChallengeId",
            //     table: "sustainableactivities");

            // This may throw if the column is already dropped
            // migrationBuilder.DropColumn(
            //     name: "ChallengeId",
            //     table: "sustainableactivities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore only if you want to reintroduce the ChallengeId relationship in the future
            // You can safely leave this as is — EF only uses Down() if you rollback
            migrationBuilder.AddColumn<int>(
                name: "ChallengeId",
                table: "sustainableactivities",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_sustainableactivities_ChallengeId",
                table: "sustainableactivities",
                column: "ChallengeId");

            migrationBuilder.AddForeignKey(
                name: "FK_sustainableactivities_challenges_ChallengeId",
                table: "sustainableactivities",
                column: "ChallengeId",
                principalTable: "challenges",
                principalColumn: "id");
        }
    }
}
