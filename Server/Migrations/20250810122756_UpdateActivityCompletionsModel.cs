using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateActivityCompletionsModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Backfill first
            migrationBuilder.Sql(@"UPDATE activitycompletions SET ""Quantity"" = 0 WHERE ""Quantity"" IS NULL;");
            migrationBuilder.Sql(@"UPDATE activitycompletions SET imagepath = 'legacy-no-image' WHERE imagepath IS NULL;");

            // Enforce NOT NULL
            migrationBuilder.AlterColumn<double>(
                name: "Quantity",
                table: "activitycompletions",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "imagepath",
                table: "activitycompletions",
                type: "text",
                nullable: false,
                defaultValue: "legacy-no-image",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Quantity",
                table: "activitycompletions",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<string>(
                name: "imagepath",
                table: "activitycompletions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
