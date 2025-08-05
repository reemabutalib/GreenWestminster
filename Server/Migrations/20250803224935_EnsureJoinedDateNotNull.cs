using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class EnsureJoinedDateNotNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AlterColumn<DateTime>(
        name: "joineddate",
        table: "userchallenges",
        type: "timestamp with time zone",
        nullable: false,
        defaultValueSql: "CURRENT_TIMESTAMP",
        oldClrType: typeof(DateTime),
        oldType: "timestamp with time zone",
        oldNullable: true);
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AlterColumn<DateTime>(
        name: "joineddate",
        table: "userchallenges",
        type: "timestamp with time zone",
        nullable: true,
        oldClrType: typeof(DateTime),
        oldType: "timestamp with time zone");
}

    }
}
