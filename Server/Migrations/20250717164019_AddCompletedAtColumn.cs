using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class AddCompletedAtColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First check if the users table is already lowercase
            migrationBuilder.Sql(@"
        DO $$
        BEGIN
            IF EXISTS (SELECT 1 FROM pg_tables WHERE tablename = 'users' AND schemaname = 'public') THEN
                -- Table is already lowercase, don't try to rename
                RAISE NOTICE 'Tables already in lowercase format';
            ELSIF EXISTS (SELECT 1 FROM pg_tables WHERE tablename = 'Users' AND schemaname = 'public') THEN
                -- Only try to rename if uppercase exists
                EXECUTE 'ALTER TABLE ""Users"" RENAME TO users;';
                EXECUTE 'ALTER TABLE ""UserChallenges"" RENAME TO userchallenges;';
                EXECUTE 'ALTER TABLE ""SustainableActivities"" RENAME TO sustainableactivities;';
                EXECUTE 'ALTER TABLE ""Challenges"" RENAME TO challenges;';
                EXECUTE 'ALTER TABLE ""ActivityCompletions"" RENAME TO activitycompletions;';
            END IF;
        END $$;
    ");

            // Add completedat column if it doesn't exist
            migrationBuilder.Sql(@"
        DO $$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                          WHERE table_name = 'userchallenges' AND column_name = 'completedat') THEN
                ALTER TABLE userchallenges ADD COLUMN completedat timestamp NULL;
            END IF;
        END $$;
    ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Don't attempt to rename tables back in the Down method
            // Just drop the completedat column if needed
            migrationBuilder.Sql(@"
        DO $$
        BEGIN
            IF EXISTS (SELECT 1 FROM information_schema.columns 
                      WHERE table_name = 'userchallenges' AND column_name = 'completedat') THEN
                ALTER TABLE userchallenges DROP COLUMN completedat;
            END IF;
        END $$;
    ");
        }
    }
}