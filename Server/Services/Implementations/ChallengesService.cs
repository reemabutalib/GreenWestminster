using Server.Models;
using Server.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server.DTOs;
using Server.Repositories.Interfaces;

namespace Server.Services.Implementations
{
    public class ChallengesService : IChallengesService
    {
        private readonly IChallengeRepository _challengeRepository;

        public ChallengesService(IChallengeRepository challengeRepository)
        {
            _challengeRepository = challengeRepository;
        }

        public async Task<IEnumerable<object>> GetChallengesAsync()
        {
            var challenges = await _challengeRepository.GetAllAsync();
            return challenges.Select(c => new
            {
                id = c.Id,
                title = c.Title,
                description = c.Description,
                startDate = c.StartDate,
                endDate = c.EndDate,
                pointsReward = c.PointsReward,
                isActive = c.StartDate <= DateTime.UtcNow && c.EndDate >= DateTime.UtcNow
            });
        }

        public async Task<IEnumerable<object>> GetActiveChallengesAsync()
        {
            var challenges = await _challengeRepository.GetAllAsync();
            var currentDate = DateTime.UtcNow.Date;
            return challenges
                .Where(c => c.StartDate <= currentDate && c.EndDate >= currentDate)
                .Select(c => new
                {
                    id = c.Id,
                    title = c.Title,
                    description = c.Description,
                    startDate = c.StartDate,
                    endDate = c.EndDate,
                    pointsReward = c.PointsReward
                });
        }

        public async Task<object?> GetChallengeByIdAsync(int id)
        {
            var c = await _challengeRepository.GetByIdAsync(id);
            if (c == null) return null;
            return new
            {
                id = c.Id,
                title = c.Title,
                description = c.Description,
                startDate = c.StartDate,
                endDate = c.EndDate,
                pointsReward = c.PointsReward,
                isActive = c.StartDate <= DateTime.UtcNow && c.EndDate >= DateTime.UtcNow
            };
        }

        public async Task<IEnumerable<object>> GetUserChallengesAsync(int userId)
        {
            var challenges = await _challengeRepository.GetAllAsync();
            return challenges
                .SelectMany(c => c.UserChallenges
                    .Where(uc => uc.UserId == userId)
                    .Select(uc => new
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
                    }))
                .ToList();
        }

        public async Task<object> CreateChallengeAsync(Challenge challenge)
        {
            // Remove invalid navigation reference
            challenge.Activities = null;

            await _challengeRepository.AddAsync(challenge);

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
    var challenge = await _challengeRepository.GetByIdAsync(challengeId);
    if (challenge == null) return false;

    if (challenge.UserChallenges.Any(uc => uc.UserId == userId))
        return false;

   var userChallenge = new UserChallenge
{
    UserId = userId,
    ChallengeId = challengeId,
    JoinedDate = DateTime.UtcNow,
    Progress = 0,
    Status = "In Progress",
    Completed = false
};

await _challengeRepository.AddUserChallengeAsync(userChallenge);


    return true;
}


        public async Task<IEnumerable<object>> GetPastChallengesAsync()
        {
            var challenges = await _challengeRepository.GetAllAsync();
            var currentDate = DateTime.UtcNow.Date;
            return challenges
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
                });
        }

        public async Task<object?> UpdateChallengeAsync(int id, Challenge challenge)
        {
            if (id != challenge.Id) return null;

            var existingChallenge = await _challengeRepository.GetByIdAsync(id);
            if (existingChallenge == null) return null;

            existingChallenge.Title = challenge.Title;
            existingChallenge.Description = challenge.Description;
            existingChallenge.StartDate = challenge.StartDate;
            existingChallenge.EndDate = challenge.EndDate;
            existingChallenge.PointsReward = challenge.PointsReward;
            existingChallenge.Category = challenge.Category;

            await _challengeRepository.UpdateAsync(existingChallenge);

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
            var challenge = await _challengeRepository.GetByIdAsync(id);
            if (challenge == null) return false;

            if (challenge.UserChallenges.Any())
            {
                challenge.UserChallenges.Clear();
                await _challengeRepository.UpdateAsync(challenge);
            }

            await _challengeRepository.DeleteAsync(id);
            return true;
        }

        public async Task<object?> UpdateChallengeStatusAsync(int id, ChallengeStatusUpdateDto statusUpdate)
        {
            var challenge = await _challengeRepository.GetByIdAsync(id);
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

            await _challengeRepository.UpdateAsync(challenge);

            return new
            {
                id = challenge.Id,
                title = challenge.Title,
                startDate = challenge.StartDate,
                endDate = challenge.EndDate,
                isActive = challenge.StartDate <= DateTime.UtcNow && challenge.EndDate >= DateTime.UtcNow
            };
        }

        public async Task<bool> AddActivitiesToChallengeAsync(int challengeId, List<int> activityIds)
{
    var challenge = await _challengeRepository.GetByIdAsync(challengeId);
    if (challenge == null) return false;

    // Optional: clear existing activities first
    // challenge.Activities.Clear();

    // Since you're avoiding includes on Activities, we don't load them — just null it out or fetch if needed
    challenge.Activities = null;

    await _challengeRepository.UpdateAsync(challenge);
    return true;
}

    }
}
