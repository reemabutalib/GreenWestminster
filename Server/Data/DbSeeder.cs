using Server.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BC = BCrypt.Net.BCrypt;

namespace Server.Data
{
    public static class DbSeeder
    {
        public static async Task SeedData(AppDbContext context)
        {
            try
            {
                // Try to connect to the database first
                Console.WriteLine("Checking database connection...");
                bool canConnect = await context.Database.CanConnectAsync();
                Console.WriteLine($"Database connection test: {(canConnect ? "Successful" : "Failed")}");
                
                if (!canConnect)
                {
                    Console.WriteLine("Cannot connect to database. Ensure connection string is correct.");
                    return;
                }
                
                // Check if tables exist before attempting to seed
                bool usersExist = false;
                bool challengesExist = false;
                bool activitiesExist = false;
                
                Console.WriteLine("Checking if required tables exist...");
                
                try
                {
                    // Check if tables exist by querying the information schema
                    var result = await context.Database.ExecuteSqlRawAsync(@"
                        SELECT COUNT(*) FROM information_schema.tables 
                        WHERE table_schema = 'public' AND table_name = 'users'
                    ");
                    usersExist = result > 0;
                    
                    Console.WriteLine($"Users table exists: {usersExist}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error checking for users table: {ex.Message}");
                }
                
                // Use EF Core migrations or schema compare instead of manual table creation
                // This is safer for a hosted environment like Render
                if (!usersExist)
                {
                    Console.WriteLine("Tables don't exist. Creating schema...");
                    try
                    {
                        await context.Database.EnsureCreatedAsync();
                        Console.WriteLine("Database schema created successfully");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error creating database schema: {ex.Message}");
                        return; // Exit if schema creation fails
                    }
                }
                
                // Check if we have any users already
                int userCount = 0;
                try
                {
                    var countResult = await context.Database.ExecuteSqlRawAsync(@"
                        SELECT COUNT(*) FROM users
                    ");
                    userCount = countResult;
                    Console.WriteLine($"Found {userCount} existing users");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error checking user count: {ex.Message}");
                    userCount = 0;
                }
                
                // Only seed if we have no users
                if (userCount == 0)
                {
                    Console.WriteLine("No existing users found. Seeding initial data...");
                    
                    // Add test user
                    try
                    {
                        var hashedPassword = BC.HashPassword("Password123!");
                        var now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                        
                        await context.Database.ExecuteSqlRawAsync($@"
                            INSERT INTO users (username, email, password, course, yearofstudy, accommodationtype, joindate, points, currentstreak, maxstreak, lastactivitydate)
                            VALUES ('testuser', 'test@westminster.ac.uk', '{hashedPassword}', 'Computer Science', 2, 'University Halls', '{now}', 50, 1, 1, '{now}')
                        ");
                        
                        Console.WriteLine("Test user added successfully");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error adding test user: {ex.Message}");
                    }
                    
                    // Add challenges
                    try
                    {
                        var now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                        var weekLater = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd HH:mm:ss");
                        
                        await context.Database.ExecuteSqlRawAsync($@"
                            INSERT INTO challenges (title, description, startdate, enddate, pointsreward, category)
                            VALUES 
                            ('Waste Reduction Challenge', 'Reduce your plastic waste by 50% this week', '{now}', '{weekLater}', 100, 'Waste Reduction'),
                            ('Green Transport Week', 'Use public transport, cycle, or walk for all your journeys this week', '{now}', '{weekLater}', 150, 'Transportation')
                        ");
                        
                        Console.WriteLine("Challenges added successfully");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error adding challenges: {ex.Message}");
                    }
                    
                    // Add sustainable activities
                    try
                    {
                        await context.Database.ExecuteSqlRawAsync(@"
                            INSERT INTO sustainableactivities (title, description, pointsvalue, category, isdaily, isweekly, isonetime)
                            VALUES 
                            ('Reusable Water Bottle', 'Use a reusable water bottle instead of buying plastic bottles', 10, 'Waste Reduction', true, false, false),
                            ('Public Transport', 'Take public transport instead of using a car', 15, 'Transportation', true, false, false)
                        ");
                        
                        Console.WriteLine("Activities added successfully");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error adding activities: {ex.Message}");
                    }
                    
                    Console.WriteLine("Database seeding complete!");
                }
                else
                {
                    Console.WriteLine("Database already contains users. Skipping seed process.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error in DbSeeder: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }
    }
}