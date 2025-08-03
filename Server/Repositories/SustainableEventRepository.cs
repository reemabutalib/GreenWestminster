using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Repositories
{
    public class SustainableEventRepository
    {
        private readonly AppDbContext _context;

        public SustainableEventRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SustainabilityEvent?> GetByIdAsync(int id)
        {
            return await _context.SustainabilityEvents
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<SustainabilityEvent>> GetAllAsync()
        {
            return await _context.SustainabilityEvents.ToListAsync();
        }

        public async Task AddAsync(SustainabilityEvent sustainabilityEvent)
        {
            await _context.SustainabilityEvents.AddAsync(sustainabilityEvent);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SustainabilityEvent sustainabilityEvent)
        {
            _context.SustainabilityEvents.Update(sustainabilityEvent);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var sustainabilityEvent = await _context.SustainabilityEvents.FindAsync(id);
            if (sustainabilityEvent != null)
            {
                _context.SustainabilityEvents.Remove(sustainabilityEvent);
                await _context.SaveChangesAsync();
            }
        }
    }
}