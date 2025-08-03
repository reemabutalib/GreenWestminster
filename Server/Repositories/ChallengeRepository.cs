using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Repositories
{
    public class ChallengeRepository : IChallengeRepository
    {
        private readonly AppDbContext _context;

        public ChallengeRepository(AppDbContext context)
        {
            _context = context;
        }

        public virtual async Task<List<Challenge>> GetAllAsync()
        {
            return await _context.Challenges
                .Include(c => c.UserChallenges)
                .Include(c => c.Activities)
                .ToListAsync();
        }

        public virtual async Task<Challenge?> GetByIdAsync(int id)
        {
            return await _context.Challenges
                .Include(c => c.UserChallenges)
                .Include(c => c.Activities)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public virtual async Task AddAsync(Challenge challenge)
        {
            await _context.Challenges.AddAsync(challenge);
            await _context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(Challenge challenge)
        {
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
    }
}