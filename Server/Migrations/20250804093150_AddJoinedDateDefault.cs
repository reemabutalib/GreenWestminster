using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class AddJoinedDateDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AlterColumn<DateTime>(
        name: "joineddate",
        table: "userchallenges",
        type: "timestamp without time zone",
        nullable: false,
        defaultValueSql: "now()",
        oldClrType: typeof(DateTime),
        oldType: "timestamp without time zone");
}


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AlterColumn<DateTime>(
        name: "joineddate",
        table: "userchallenges",
        type: "timestamp without time zone",
        nullable: false,
        oldClrType: typeof(DateTime),
        oldType: "timestamp without time zone",
        oldDefaultValueSql: "now()");
}

    }
}
