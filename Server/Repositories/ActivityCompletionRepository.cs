using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;
using Server.Services.Interfaces;

namespace Server.Repositories
{
    public class ActivityCompletionRepository : IActivityCompletionRepository
    {
        private readonly AppDbContext _context;

        public ActivityCompletionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ActivityCompletion?> GetByIdAsync(int id)
        {
            return await _context.ActivityCompletions
                .Include(ac => ac.User)
                .Include(ac => ac.Activity)
                .FirstOrDefaultAsync(ac => ac.Id == id);
        }

        public async Task<List<ActivityCompletion>> GetByUserIdAsync(int userId)
        {
            return await _context.ActivityCompletions
                .Where(ac => ac.UserId == userId)
                .Include(ac => ac.Activity)
                .ToListAsync();
        }

        public async Task<List<ActivityCompletion>> GetAllAsync()
        {
            return await _context.ActivityCompletions
                .Include(ac => ac.User)
                .Include(ac => ac.Activity)
                .ToListAsync();
        }

        public async Task AddAsync(ActivityCompletion completion)
        {
            await _context.ActivityCompletions.AddAsync(completion);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ActivityCompletion completion)
        {
            _context.ActivityCompletions.Update(completion);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var completion = await _context.ActivityCompletions.FindAsync(id);
            if (completion != null)
            {
                _context.ActivityCompletions.Remove(completion);
                await _context.SaveChangesAsync();
            }
        }
    }
}