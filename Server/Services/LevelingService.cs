using System;
using System.Collections.Generic;

namespace Server.Services
{
    public class LevelingService
    {
        private static readonly List<(string Level, int Threshold)> OrderedLevels = new List<(string, int)>
        {
            ("Bronze", 0),
            ("Silver", 500),
            ("Gold", 1000),
            ("Platinum", 5000)
        };

        public static string CalculateLevel(int points)
        {
            string level = "Bronze";
            foreach (var entry in OrderedLevels)
            {
                if (points >= entry.Threshold)
                {
                    level = entry.Level;
                }
                else
                {
                    break;
                }
            }
            return level;
        }


        // Calculate points needed for the next level
        public static int PointsToNextLevel(int currentPoints)
        {
            foreach (var entry in OrderedLevels)
            {
                if (currentPoints < entry.Threshold)
                {
                    return entry.Threshold - currentPoints;
                }
            }
            // If at max level, return 0
            return 0;
        }

        // Get the total points needed for a specific level
        public static int GetPointsForLevel(string level)
        {
            foreach (var entry in OrderedLevels)
            {
                if (entry.Level == level)
                    return entry.Threshold;
            }
            return -1;
        }

        // Get all levels in order
        public static List<string> GetAllLevels()
        {
            var levels = new List<string>();
            foreach (var entry in OrderedLevels)
            {
                levels.Add(entry.Level);
            }
            return levels;
        }
    }
}