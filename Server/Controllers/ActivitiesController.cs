using Server.Data;
using Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivitiesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ActivitiesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/activities
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SustainableActivity>>> GetActivities()
    {
        return await _context.SustainableActivities.ToListAsync();
    }

    // GET: api/activities/5
    [HttpGet("{id}")]
    public async Task<ActionResult<SustainableActivity>> GetActivity(int id)
    {
        var activity = await _context.SustainableActivities.FindAsync(id);

        if (activity == null)
        {
            return NotFound();
        }

        return activity;
    }

    // POST: api/activities
    [HttpPost]
    public async Task<ActionResult<SustainableActivity>> CreateActivity(SustainableActivity activity)
    {
        _context.SustainableActivities.Add(activity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetActivity), new { id = activity.Id }, activity);
    }

    // GET: api/activities/category/waste-reduction
    [HttpGet("category/{category}")]
    public async Task<ActionResult<IEnumerable<SustainableActivity>>> GetActivitiesByCategory(string category)
    {
        return await _context.SustainableActivities
            .Where(a => a.Category.ToLower() == category.ToLower())
            .ToListAsync();
    }
}