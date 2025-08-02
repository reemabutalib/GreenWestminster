using Server.Data;
using Server.Models;
using Microsoft.EntityFrameworkCore;
using Server.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server.DTOs;

namespace Server.Services.Implementations
{
    public class ChallengesService : IChallengesService
    {
        private readonly AppDbContext _context;

        public ChallengesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>> GetChallengesAsync()
        {
            return await _context.Challenges
                .Select(c => new
                {
                    id = c.Id,
                    title = c.Title,
                    description = c.Description,
                    startDate = c.StartDate,
                    endDate = c.EndDate,
                    pointsReward = c.PointsReward,
                    isActive = c.StartDate <= DateTime.UtcNow && c.EndDate >= DateTime.UtcNow
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetActiveChallengesAsync()
        {
            var currentDate = DateTime.UtcNow.Date;
            return await _context.Challenges
                .Where(c => c.StartDate <= currentDate && c.EndDate >= currentDate)
                .Select(c => new
                {
                    id = c.Id,
                    title = c.Title,
                    description = c.Description,
                    startDate = c.StartDate,
                    endDate = c.EndDate,
                    pointsReward = c.PointsReward
                })
                .ToListAsync();
        }

        public async Task<object?> GetChallengeByIdAsync(int id)
        {
            return await _context.Challenges
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    id = c.Id,
                    title = c.Title,
                    description = c.Description,
                    startDate = c.StartDate,
                    endDate = c.EndDate,
                    pointsReward = c.PointsReward,
                    isActive = c.StartDate <= DateTime.UtcNow && c.EndDate >= DateTime.UtcNow
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<object>> GetUserChallengesAsync(int userId)
        {
            return await _context.UserChallenges
                .Where(uc => uc.UserId == userId)
                .Join(_context.Challenges,
                    uc => uc.ChallengeId,
                    c => c.Id,
                    (uc, c) => new
                    {
                        id = c.Id,
                        title = c.Title,
                        description = c.Description,
                        startDate = c.StartDate,
                        endDate = c.EndDate,
                        pointsReward = c.PointsReward,
                        progress = uc.Progress,
                        status = uc.Status,
                        joinedDate = uc.JoinedDate
                    })
                .ToListAsync();
        }

        public async Task<object> CreateChallengeAsync(Challenge challenge)
        {
            if (challenge.Activities != null && challenge.Activities.Any())
            {
                challenge.Activities = null;
            }

            _context.Challenges.Add(challenge);
            await _context.SaveChangesAsync();

            return new
            {
                id = challenge.Id,
                title = challenge.Title,
                description = challenge.Description,
                startDate = challenge.StartDate,
                endDate = challenge.EndDate,
                pointsReward = challenge.PointsReward
            };
        }

        public async Task<bool> JoinChallengeAsync(int challengeId, int userId)
        {
            var challengeExists = await _context.Challenges.AnyAsync(c => c.Id == challengeId);
            if (!challengeExists) return false;

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists) return false;

            var alreadyJoined = await _context.UserChallenges
                .AnyAsync(uc => uc.UserId == userId && uc.ChallengeId == challengeId);

            if (alreadyJoined) return false;

            var userChallenge = new UserChallenge
            {
                UserId = userId,
                ChallengeId = challengeId,
                JoinedDate = DateTime.UtcNow,
                Progress = 0,
                Status = "In Progress",
                Completed = false
            };

            _context.UserChallenges.Add(userChallenge);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AddActivitiesToChallengeAsync(int challengeId, List<int> activityIds)
        {
            var challenge = await _context.Challenges.FindAsync(challengeId);
            if (challenge == null) return false;

            foreach (var activityId in activityIds)
            {
                var activity = await _context.SustainableActivities.FindAsync(activityId);
                if (activity == null)
                {
                    return false;
                }   
            }

            return true;
        }

        public async Task<IEnumerable<object>> GetPastChallengesAsync()
        {
            var currentDate = DateTime.UtcNow.Date;
            return await _context.Challenges
                .Where(c => c.EndDate < currentDate)
                .Select(c => new
                {
                    id = c.Id,
                    title = c.Title,
                    description = c.Description,
                    category = c.Category,
                    startDate = c.StartDate,
                    endDate = c.EndDate,
                    pointsReward = c.PointsReward
                })
                .ToListAsync();
        }

        public async Task<object?> UpdateChallengeAsync(int id, Challenge challenge)
        {
            if (id != challenge.Id) return null;

            var existingChallenge = await _context.Challenges.FindAsync(id);
            if (existingChallenge == null) return null;

            existingChallenge.Title = challenge.Title;
            existingChallenge.Description = challenge.Description;
            existingChallenge.StartDate = challenge.StartDate;
            existingChallenge.EndDate = challenge.EndDate;
            existingChallenge.PointsReward = challenge.PointsReward;
            existingChallenge.Category = challenge.Category;

            if (challenge.Activities != null)
            {
                existingChallenge.Activities = null;
            }

            _context.Entry(existingChallenge).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return new
            {
                id = existingChallenge.Id,
                title = existingChallenge.Title,
                description = existingChallenge.Description,
                startDate = existingChallenge.StartDate,
                endDate = existingChallenge.EndDate,
                pointsReward = existingChallenge.PointsReward,
                category = existingChallenge.Category,
                isActive = existingChallenge.StartDate <= DateTime.UtcNow && existingChallenge.EndDate >= DateTime.UtcNow
            };
        }

        public async Task<bool> DeleteChallengeAsync(int id)
        {
            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge == null) return false;

            var userParticipations = await _context.UserChallenges
                .Where(uc => uc.ChallengeId == id)
                .ToListAsync();

            if (userParticipations.Any())
            {
                _context.UserChallenges.RemoveRange(userParticipations);
                await _context.SaveChangesAsync();
            }

            _context.Challenges.Remove(challenge);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<object?> UpdateChallengeStatusAsync(int id, ChallengeStatusUpdateDto statusUpdate)
        {
            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge == null) return null;

            if (statusUpdate.IsActive.HasValue)
            {
                if (statusUpdate.IsActive.Value)
                {
                    if (statusUpdate.StartDate.HasValue)
                        challenge.StartDate = statusUpdate.StartDate.Value;
                    else if (challenge.StartDate > DateTime.UtcNow)
                        challenge.StartDate = DateTime.UtcNow;

                    if (statusUpdate.EndDate.HasValue)
                        challenge.EndDate = statusUpdate.EndDate.Value;
                    else if (challenge.EndDate < DateTime.UtcNow)
                        challenge.EndDate = DateTime.UtcNow.AddDays(30);
                }
                else if (statusUpdate.EndNow == true)
                {
                    challenge.EndDate = DateTime.UtcNow;
                }
            }
            else
            {
                if (statusUpdate.StartDate.HasValue)
                    challenge.StartDate = statusUpdate.StartDate.Value;
                if (statusUpdate.EndDate.HasValue)
                    challenge.EndDate = statusUpdate.EndDate.Value;
            }

            await _context.SaveChangesAsync();

            return new
            {
                id = challenge.Id,
                title = challenge.Title,
                startDate = challenge.StartDate,
                endDate = challenge.EndDate,
                isActive = challenge.StartDate <= DateTime.UtcNow && challenge.EndDate >= DateTime.UtcNow
            };
        }
    }
}