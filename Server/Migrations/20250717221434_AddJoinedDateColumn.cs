using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class AddJoinedDateColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add all missing columns safely in a single PL/pgSQL block
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    -- Add joineddate column if missing
                    IF NOT EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_name = 'userchallenges' 
                        AND column_name = 'joineddate'
                    ) THEN
                        ALTER TABLE userchallenges 
                        ADD COLUMN joineddate timestamp without time zone DEFAULT CURRENT_TIMESTAMP;
                    END IF;

                    -- Add completedat column if missing
                    IF NOT EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_name = 'userchallenges' 
                        AND column_name = 'completedat'
                    ) THEN
                        ALTER TABLE userchallenges 
                        ADD COLUMN completedat timestamp without time zone NULL;
                    END IF;
                    
                    -- Add progress column if missing
                    IF NOT EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_name = 'userchallenges' 
                        AND column_name = 'progress'
                    ) THEN
                        ALTER TABLE userchallenges 
                        ADD COLUMN progress integer NOT NULL DEFAULT 0;
                    END IF;
                    
                    -- Add status column if missing
                    IF NOT EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_name = 'userchallenges' 
                        AND column_name = 'status'
                    ) THEN
                        ALTER TABLE userchallenges 
                        ADD COLUMN status text NOT NULL DEFAULT 'In Progress';
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Safe removal of all columns if they exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_name = 'userchallenges' 
                        AND column_name = 'joineddate'
                    ) THEN
                        ALTER TABLE userchallenges DROP COLUMN joineddate;
                    END IF;
                    
                    IF EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_name = 'userchallenges' 
                        AND column_name = 'completedat'
                    ) THEN
                        ALTER TABLE userchallenges DROP COLUMN completedat;
                    END IF;
                    
                    IF EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_name = 'userchallenges' 
                        AND column_name = 'progress'
                    ) THEN
                        ALTER TABLE userchallenges DROP COLUMN progress;
                    END IF;
                    
                    IF EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_name = 'userchallenges' 
                        AND column_name = 'status'
                    ) THEN
                        ALTER TABLE userchallenges DROP COLUMN status;
                    END IF;
                END $$;
            ");
        }
    }
}