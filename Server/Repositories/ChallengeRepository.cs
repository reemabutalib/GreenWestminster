using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;
using Server.Repositories.Interfaces;

namespace Server.Repositories
{
    public class ChallengeRepository : IChallengeRepository
    {
        private readonly AppDbContext _context;

        public ChallengeRepository(AppDbContext context)
        {
            _context = context;
        }

        public virtual async Task AddAsync(Challenge challenge)
        {
            await _context.Challenges.AddAsync(challenge);
            await _context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(Challenge challenge)
        {
            // Ensure each related UserChallenge has a valid JoinedDate
            if (challenge.UserChallenges != null)
            {
                foreach (var uc in challenge.UserChallenges)
                {
                    if (uc.JoinedDate == default)
                    {
                        uc.JoinedDate = DateTime.UtcNow;
                    }
                }
            }

            _context.Challenges.Update(challenge);
            await _context.SaveChangesAsync();
        }


        public virtual async Task DeleteAsync(int id)
        {
            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge != null)
            {
                _context.Challenges.Remove(challenge);
                await _context.SaveChangesAsync();
            }
        }

        public virtual async Task<UserChallenge?> GetUserChallengeAsync(int userId, int challengeId)
        {
            return await _context.UserChallenges
                .Include(uc => uc.Challenge)
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ChallengeId == challengeId);
        }

        public virtual async Task<IEnumerable<UserChallenge>> GetCompletedChallengesByUserIdAsync(int userId)
        {
            return await _context.UserChallenges
                .Include(uc => uc.Challenge)
                .Where(uc => uc.UserId == userId && uc.IsCompleted)
                .ToListAsync();
        }

        public async Task<List<Challenge>> GetAllAsync()
        {
            return await _context.Challenges
                .Include(c => c.UserChallenges)
                .ToListAsync();
        }

        public async Task<Challenge?> GetByIdAsync(int id)
        {
            return await _context.Challenges
                .Include(c => c.UserChallenges)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

public async Task AddUserChallengeAsync(UserChallenge userChallenge)
{
    // Log everything
    Console.WriteLine($"[DEBUG] Adding UserChallenge:");
    Console.WriteLine($"  UserId: {userChallenge.UserId}");
    Console.WriteLine($"  ChallengeId: {userChallenge.ChallengeId}");
    Console.WriteLine($"  JoinedDate: {userChallenge.JoinedDate}");
    Console.WriteLine($"  Progress: {userChallenge.Progress}");
    Console.WriteLine($"  Status: {userChallenge.Status}");
    Console.WriteLine($"  Completed: {userChallenge.Completed}");

    _context.UserChallenges.Add(userChallenge);
    await _context.SaveChangesAsync();
}



    }
}