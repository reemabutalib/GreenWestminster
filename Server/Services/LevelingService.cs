using System;
using System.Collections.Generic;

namespace Server.Services
{
    public class LevelingService
    {
        // Define the points thresholds for each level
        private static readonly Dictionary<int, int> LevelThresholds = new Dictionary<int, int>
        {
            { 1, 0 },     // Level 1 starts at 0 points
            { 2, 100 },   // Level 2 requires 100 points
            { 3, 250 },   // Level 3 requires 250 points
            { 4, 500 },   // Level 4 requires 500 points
            { 5, 1000 }   // Level 5 requires 1000 points
        };

        // Calculate the level based on points
        public static int CalculateLevel(int points)
        {
            int level = 1; // Default level
            
            foreach (var threshold in LevelThresholds)
            {
                if (points >= threshold.Value)
                {
                    level = threshold.Key;
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
            int currentLevel = CalculateLevel(currentPoints);
            int nextLevel = currentLevel + 1;
            
            // If we're at the max level, return 0
            if (!LevelThresholds.ContainsKey(nextLevel))
            {
                return 0;
            }
            
            return LevelThresholds[nextLevel] - currentPoints;
        }
        
        // Get the total points needed for a specific level
        public static int GetPointsForLevel(int level)
        {
            return LevelThresholds.ContainsKey(level) ? LevelThresholds[level] : -1;
        }
    }
}