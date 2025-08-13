using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class ImageFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Allow NULLs on imagepath
            migrationBuilder.Sql(@"
                ALTER TABLE activitycompletions
                ALTER COLUMN imagepath DROP NOT NULL;
            ");

            // 2) Normalize legacy values to NULL (blank strings / legacy placeholder)
            migrationBuilder.Sql(@"
                UPDATE activitycompletions
                SET imagepath = NULL
                WHERE imagepath IS NULL
                   OR btrim(imagepath) = ''
                   OR lower(btrim(imagepath)) = 'legacy-no-image';
            ");

            // 3) Add a CHECK to prevent blank (but still allow NULL for legacy rows)
            //    Use a DO block so re-running doesn’t error if the constraint already exists.
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conname = 'imagepath_not_blank'
                    ) THEN
                        ALTER TABLE activitycompletions
                        ADD CONSTRAINT imagepath_not_blank
                        CHECK (imagepath IS NULL OR length(btrim(imagepath)) > 0);
                    END IF;
                END$$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1) Drop the CHECK constraint if it exists
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conname = 'imagepath_not_blank'
                    ) THEN
                        ALTER TABLE activitycompletions
                        DROP CONSTRAINT imagepath_not_blank;
                    END IF;
                END$$;
            ");

            // 2) Replace NULLs with empty string so we can enforce NOT NULL again
            migrationBuilder.Sql(@"
                UPDATE activitycompletions
                SET imagepath = ''
                WHERE imagepath IS NULL;
            ");

            // 3) Reinstate NOT NULL on imagepath
            migrationBuilder.Sql(@"
                ALTER TABLE activitycompletions
                ALTER COLUMN imagepath SET NOT NULL;
            ");
        }
    }
}
