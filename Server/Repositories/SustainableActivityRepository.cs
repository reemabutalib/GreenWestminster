using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;
using Server.Repositories.Interfaces;

namespace Server.Repositories
{
    public class SustainableActivityRepository : ISustainableActivityRepository
    {
        private readonly AppDbContext _context;

        public SustainableActivityRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SustainableActivity?> GetByIdAsync(int id)
        {
            return await _context.SustainableActivities.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<SustainableActivity>> GetAllAsync()
        {
            return await _context.SustainableActivities.ToListAsync();
        }

        public async Task AddAsync(SustainableActivity activity)
        {
            await _context.SustainableActivities.AddAsync(activity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SustainableActivity activity)
        {
            _context.SustainableActivities.Update(activity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var activity = await _context.SustainableActivities.FindAsync(id);
            if (activity != null)
            {
                _context.SustainableActivities.Remove(activity);
                await _context.SaveChangesAsync();
            }
        }
    }
}