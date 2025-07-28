using Server.Data;
using Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Npgsql;
using System.IO;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EventsController> _logger;

        public EventsController(AppDbContext context, ILogger<EventsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetEvents()
        {
            try
            {
                _logger.LogInformation("Fetching all sustainability events");

                var events = await _context.SustainabilityEvents
                    .AsNoTracking()
                    .Select(e => new
                    {
                        id = e.Id,
                        title = e.Title,
                        description = e.Description,
                        location = e.Location,
                        startDate = e.StartDate,
                        endDate = e.EndDate,
                        registrationLink = e.RegistrationLink,
                        imageUrl = e.ImageUrl,
                        organizer = e.Organizer,
                        maxAttendees = e.MaxAttendees,
                        category = e.Category,
                        isVirtual = e.IsVirtual,
                        createdAt = e.CreatedAt,
                        updatedAt = e.UpdatedAt
                    })
                    .OrderBy(e => e.startDate)
                    .ToListAsync();

                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching events");
                return StatusCode(500, new { message = "An error occurred while retrieving events", error = ex.Message });
            }
        }

        // GET: api/events/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetEvent(int id)
        {
            try
            {
                _logger.LogInformation("Fetching event with ID: {Id}", id);

                var sustainabilityEvent = await _context.SustainabilityEvents
                    .AsNoTracking()
                    .Where(e => e.Id == id)
                    .Select(e => new
                    {
                        id = e.Id,
                        title = e.Title,
                        description = e.Description,
                        location = e.Location,
                        startDate = e.StartDate,
                        endDate = e.EndDate,
                        registrationLink = e.RegistrationLink,
                        imageUrl = e.ImageUrl,
                        organizer = e.Organizer,
                        maxAttendees = e.MaxAttendees,
                        category = e.Category,
                        isVirtual = e.IsVirtual,
                        createdAt = e.CreatedAt,
                        updatedAt = e.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (sustainabilityEvent == null)
                {
                    _logger.LogWarning("Event with ID: {Id} not found", id);
                    return NotFound(new { message = $"Event with ID: {id} not found" });
                }

                return Ok(sustainabilityEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching event with ID: {Id}", id);
                return StatusCode(500, new { message = $"An error occurred while retrieving event with ID: {id}", error = ex.Message });
            }
        }

        // GET: api/events/upcoming
        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<object>>> GetUpcomingEvents()
        {
            try
            {
                _logger.LogInformation("Fetching upcoming sustainability events");

                var today = DateTime.UtcNow;

                var events = await _context.SustainabilityEvents
                    .AsNoTracking()
                    .Where(e => e.StartDate > today)
                    .Select(e => new
                    {
                        id = e.Id,
                        title = e.Title,
                        description = e.Description,
                        location = e.Location,
                        startDate = e.StartDate,
                        endDate = e.EndDate,
                        registrationLink = e.RegistrationLink,
                        imageUrl = e.ImageUrl,
                        organizer = e.Organizer,
                        maxAttendees = e.MaxAttendees,
                        category = e.Category,
                        isVirtual = e.IsVirtual
                    })
                    .OrderBy(e => e.startDate)
                    .ToListAsync();

                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching upcoming events");
                return StatusCode(500, new { message = "An error occurred while retrieving upcoming events", error = ex.Message });
            }
        }

        // POST: api/events
        [HttpPost]
        [Authorize(Policy = "AdminPolicy")]
        [RequestSizeLimit(10 * 1024 * 1024)] // Limit to 10MB
        [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
        public async Task<ActionResult<object>> CreateEvent([FromForm] CreateEventDto eventDto)
        {
            try
            {
                _logger.LogInformation("Admin creating new event: {Title}", eventDto.Title);

                if (eventDto == null)
                {
                    return BadRequest(new { message = "Event data is required" });
                }

                // Handle image upload if provided
                string imageFileName = null;
                if (eventDto.Image != null && eventDto.Image.Length > 0)
                {
                    // Generate a unique filename with timestamp
                    var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
                    var fileExtension = Path.GetExtension(eventDto.Image.FileName);
                    imageFileName = $"event_{timestamp}{fileExtension}";

                    // Create uploads directory if it doesn't exist
                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "events");
                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    // Save the file
                    var filePath = Path.Combine(uploadsDir, imageFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await eventDto.Image.CopyToAsync(stream);
                    }

                    _logger.LogInformation("Saved image {ImageFileName} for event", imageFileName);
                }

                // Construct the image URL if an image was uploaded
                string imageUrl = null;
                if (!string.IsNullOrEmpty(imageFileName))
                {
                    imageUrl = $"{Request.Scheme}://{Request.Host}/uploads/events/{imageFileName}";
                }

                // Create event with fields from the DTO
                var sql = @"
                    INSERT INTO sustainabilityevents 
                        (title, description, location, startdate, enddate, registrationlink, imageurl, organizer, maxattendees, category, isvirtual, createdat, updatedat)
                    VALUES 
                        (@title, @description, @location, @startDate, @endDate, @registrationLink, @imageUrl, @organizer, @maxAttendees, @category, @isVirtual, @createdAt, @updatedAt)
                    RETURNING id";

                var currentTime = DateTime.UtcNow;
                var parameters = new[]
                {
                    new NpgsqlParameter("title", eventDto.Title),
                    new NpgsqlParameter("description", eventDto.Description),
                    new NpgsqlParameter("location", eventDto.Location ?? (object)DBNull.Value),
                    new NpgsqlParameter("startDate", eventDto.StartDate),
                    new NpgsqlParameter("endDate", eventDto.EndDate ?? (object)DBNull.Value),
                    new NpgsqlParameter("registrationLink", eventDto.RegistrationLink ?? (object)DBNull.Value),
                    new NpgsqlParameter("imageUrl", imageUrl ?? (object)DBNull.Value),
                    new NpgsqlParameter("organizer", eventDto.Organizer ?? (object)DBNull.Value),
                    new NpgsqlParameter("maxAttendees", eventDto.MaxAttendees.HasValue ? eventDto.MaxAttendees.Value : (object)DBNull.Value),
                    new NpgsqlParameter("category", eventDto.Category ?? (object)DBNull.Value),
                    new NpgsqlParameter("isVirtual", eventDto.IsVirtual),
                    new NpgsqlParameter("createdAt", currentTime),
                    new NpgsqlParameter("updatedAt", currentTime)
                };

                // Execute the SQL directly to bypass EF Core's mapping
                int newEventId;
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = sql;
                    foreach (var param in parameters)
                    {
                        command.Parameters.Add(param);
                    }

                    if (command.Connection.State != System.Data.ConnectionState.Open)
                    {
                        command.Connection.Open();
                    }

                    // Get the ID of the newly inserted event
                    newEventId = Convert.ToInt32(await command.ExecuteScalarAsync());
                }

                // Return the created event
                var createdEvent = new
                {
                    id = newEventId,
                    title = eventDto.Title,
                    description = eventDto.Description,
                    location = eventDto.Location,
                    startDate = eventDto.StartDate,
                    endDate = eventDto.EndDate,
                    registrationLink = eventDto.RegistrationLink,
                    imageUrl = imageUrl,
                    organizer = eventDto.Organizer,
                    maxAttendees = eventDto.MaxAttendees,
                    category = eventDto.Category,
                    isVirtual = eventDto.IsVirtual,
                    createdAt = currentTime,
                    updatedAt = currentTime
                };

                return CreatedAtAction(nameof(GetEvent), new { id = newEventId }, createdEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating event: {Error}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while creating the event", error = ex.Message });
            }
        }

        // PUT: api/events/5
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminPolicy")]
        [RequestSizeLimit(10 * 1024 * 1024)] // Limit to 10MB
        [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
        public async Task<ActionResult<object>> UpdateEvent(int id, [FromForm] UpdateEventDto eventDto)
        {
            try
            {
                _logger.LogInformation("Admin updating event: {Id}", id);

                if (eventDto == null)
                {
                    return BadRequest(new { message = "Event data is required" });
                }

                // Check if event exists - fixed query to avoid GROUP BY issue
                var checkSql = "SELECT id, imageurl FROM sustainabilityevents WHERE id = @id";
                var checkParam = new NpgsqlParameter("id", id);

                string currentImageUrl = null;
                bool exists = false;

                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = checkSql;
                    command.Parameters.Add(checkParam);

                    if (command.Connection.State != System.Data.ConnectionState.Open)
                    {
                        command.Connection.Open();
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            exists = true;
                            currentImageUrl = reader.IsDBNull(1) ? null : reader.GetString(1);
                        }
                    }
                }

                if (!exists)
                {
                    return NotFound(new { message = $"Event with ID: {id} not found" });
                }

                // Handle image upload if provided
                string imageUrl = currentImageUrl;
                if (eventDto.Image != null && eventDto.Image.Length > 0)
                {
                    // Delete old image if it exists
                    if (!string.IsNullOrEmpty(currentImageUrl))
                    {
                        try
                        {
                            var oldImagePath = currentImageUrl.Substring(currentImageUrl.IndexOf("/uploads/events/"));
                            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldImagePath);
                            if (System.IO.File.Exists(fullPath))
                            {
                                System.IO.File.Delete(fullPath);
                                _logger.LogInformation("Deleted old image at {ImagePath}", fullPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete old event image");
                        }
                    }

                    // Generate a unique filename with timestamp
                    var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
                    var fileExtension = Path.GetExtension(eventDto.Image.FileName);
                    var imageFileName = $"event_{timestamp}{fileExtension}";

                    // Create uploads directory if it doesn't exist
                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "events");
                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    // Save the file
                    var filePath = Path.Combine(uploadsDir, imageFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await eventDto.Image.CopyToAsync(stream);
                    }

                    imageUrl = $"{Request.Scheme}://{Request.Host}/uploads/events/{imageFileName}";
                    _logger.LogInformation("Saved new image {ImageURL} for event", imageUrl);
                }

                // Update the event with direct SQL to avoid mapping issues
                var updateSql = @"
                    UPDATE sustainabilityevents
                    SET 
                        title = @title, 
                        description = @description, 
                        location = @location, 
                        startdate = @startDate, 
                        enddate = @endDate, 
                        registrationlink = @registrationLink, 
                        imageurl = @imageUrl, 
                        organizer = @organizer, 
                        maxattendees = @maxAttendees, 
                        category = @category, 
                        isvirtual = @isVirtual,
                        updatedat = @updatedAt
                    WHERE id = @id";

                var parameters = new[]
                {
                    new NpgsqlParameter("title", eventDto.Title),
                    new NpgsqlParameter("description", eventDto.Description),
                    new NpgsqlParameter("location", eventDto.Location ?? (object)DBNull.Value),
                    new NpgsqlParameter("startDate", eventDto.StartDate),
                    new NpgsqlParameter("endDate", eventDto.EndDate ?? (object)DBNull.Value),
                    new NpgsqlParameter("registrationLink", eventDto.RegistrationLink ?? (object)DBNull.Value),
                    new NpgsqlParameter("imageUrl", imageUrl ?? (object)DBNull.Value),
                    new NpgsqlParameter("organizer", eventDto.Organizer ?? (object)DBNull.Value),
                    new NpgsqlParameter("maxAttendees", eventDto.MaxAttendees.HasValue ? eventDto.MaxAttendees.Value : (object)DBNull.Value),
                    new NpgsqlParameter("category", eventDto.Category ?? (object)DBNull.Value),
                    new NpgsqlParameter("isVirtual", eventDto.IsVirtual),
                    new NpgsqlParameter("updatedAt", DateTime.UtcNow),
                    new NpgsqlParameter("id", id)
                };

                await _context.Database.ExecuteSqlRawAsync(updateSql, parameters);

                // Return the updated event
                return Ok(new
                {
                    success = true,
                    event_ = new
                    {
                        id = id,
                        title = eventDto.Title,
                        description = eventDto.Description,
                        location = eventDto.Location,
                        startDate = eventDto.StartDate,
                        endDate = eventDto.EndDate,
                        registrationLink = eventDto.RegistrationLink,
                        imageUrl = imageUrl,
                        organizer = eventDto.Organizer,
                        maxAttendees = eventDto.MaxAttendees,
                        category = eventDto.Category,
                        isVirtual = eventDto.IsVirtual,
                        updatedAt = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating event: {Error}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while updating the event", error = ex.Message });
            }
        }

        // DELETE: api/events/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            try
            {
                _logger.LogInformation("Admin deleting event with ID: {Id}", id);

                // Get event image URL before deleting
                var getImageSql = "SELECT imageurl FROM sustainabilityevents WHERE id = @id";
                var getImageParam = new NpgsqlParameter("id", id);

                string imageUrl = null;
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = getImageSql;
                    command.Parameters.Add(getImageParam);

                    if (command.Connection.State != System.Data.ConnectionState.Open)
                    {
                        command.Connection.Open();
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync() && !reader.IsDBNull(0))
                        {
                            imageUrl = reader.GetString(0);
                        }
                        else
                        {
                            _logger.LogWarning("Event with ID: {Id} not found during delete operation", id);
                            return NotFound(new { message = $"Event with ID: {id} not found" });
                        }
                    }
                }

                // Delete the event
                var deleteEventSql = "DELETE FROM sustainabilityevents WHERE id = @id";
                var deleteEventParam = new NpgsqlParameter("id", id);

                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(deleteEventSql, deleteEventParam);

                if (rowsAffected == 0)
                {
                    _logger.LogWarning("Event with ID: {Id} not found during delete operation", id);
                    return NotFound(new { message = $"Event with ID: {id} not found" });
                }

                // Delete associated image file if it exists
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    try
                    {
                        var imagePath = imageUrl.Substring(imageUrl.IndexOf("/uploads/events/"));
                        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath);
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                            _logger.LogInformation("Deleted image at {ImagePath}", fullPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete event image, but event was deleted");
                    }
                }

                _logger.LogInformation("Event with ID: {Id} successfully deleted", id);

                return Ok(new { message = $"Event with ID: {id} successfully deleted" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting event: {Error}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while deleting the event", error = ex.Message });
            }
        }

        // GET: api/events/category/{category}
        [HttpGet("category/{category}")]
        public async Task<ActionResult<IEnumerable<object>>> GetEventsByCategory(string category)
        {
            try
            {
                _logger.LogInformation("Fetching events by category: {Category}", category);

                var events = await _context.SustainabilityEvents
                    .AsNoTracking()
                    .Where(e => EF.Functions.ILike(e.Category, $"%{category}%"))
                    .Select(e => new
                    {
                        id = e.Id,
                        title = e.Title,
                        description = e.Description,
                        location = e.Location,
                        startDate = e.StartDate,
                        endDate = e.EndDate,
                        registrationLink = e.RegistrationLink,
                        imageUrl = e.ImageUrl,
                        organizer = e.Organizer,
                        maxAttendees = e.MaxAttendees,
                        category = e.Category,
                        isVirtual = e.IsVirtual
                    })
                    .OrderBy(e => e.startDate)
                    .ToListAsync();

                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching events by category: {Category}", category);
                return StatusCode(500, new { message = $"An error occurred while retrieving events for category: {category}", error = ex.Message });
            }
        }

        // DTOs for events
        public class CreateEventDto
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string? Location { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string? RegistrationLink { get; set; }
            public IFormFile? Image { get; set; }
            public string? Organizer { get; set; }
            public int? MaxAttendees { get; set; }
            public string? Category { get; set; }
            public bool IsVirtual { get; set; } = false;
        }

        public class UpdateEventDto
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string? Location { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string? RegistrationLink { get; set; }
            public IFormFile? Image { get; set; }
            public string? Organizer { get; set; }
            public int? MaxAttendees { get; set; }
            public string? Category { get; set; }
            public bool IsVirtual { get; set; }
        }
    }
}