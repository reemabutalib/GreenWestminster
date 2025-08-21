using Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Server.Services.Interfaces;
using System.IO;
using Server.DTOs;
using Server.Data;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventsService _eventsService;
        private readonly ILogger<EventsController> _logger;
        private readonly AppDbContext _context;

        public EventsController(IEventsService eventsService, ILogger<EventsController> logger)
        {
            _eventsService = eventsService;
            _logger = logger;
        }

        // GET: api/events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetEvents()
        {
            try
            {
                var events = await _eventsService.GetEventsAsync();
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
                var sustainabilityEvent = await _eventsService.GetEventByIdAsync(id);
                if (sustainabilityEvent == null)
                    return NotFound(new { message = $"Event with ID: {id} not found" });

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
                var events = await _eventsService.GetUpcomingEventsAsync();
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
        [RequestSizeLimit(10 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
        public async Task<ActionResult<object>> CreateEvent([FromForm] CreateEventDto eventDto)
        {
            try
            {
                if (eventDto == null)
                    return BadRequest(new { message = "Event data is required" });

                string requestScheme = Request.Scheme;
                string requestHost = Request.Host.Value;

                async Task<string?> ImageHandler(IFormFile? image)
                {
                    if (image == null || image.Length == 0) return null;
                    var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
                    var fileExtension = Path.GetExtension(image.FileName);
                    var imageFileName = $"event_{timestamp}{fileExtension}";
                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "events");
                    if (!Directory.Exists(uploadsDir))
                        Directory.CreateDirectory(uploadsDir);
                    var filePath = Path.Combine(uploadsDir, imageFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    return $"/uploads/events/{imageFileName}";

                }

                var createdEvent = await _eventsService.CreateEventAsync(eventDto, ImageHandler, requestScheme, requestHost);
                return CreatedAtAction(nameof(GetEvent), new { id = ((dynamic)createdEvent).id }, createdEvent);
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
        [RequestSizeLimit(10 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
        public async Task<ActionResult<object>> UpdateEvent(int id, [FromForm] UpdateEventDto eventDto)
        {
            try
            {
                if (eventDto == null)
                    return BadRequest(new { message = "Event data is required" });

                string requestScheme = Request.Scheme;
                string requestHost = Request.Host.Value;

                async Task<string?> ImageHandler(IFormFile? image, string? currentImageUrl)
                {
                    if (image == null || image.Length == 0) return currentImageUrl;
                    // Delete old image if it exists
                    if (!string.IsNullOrEmpty(currentImageUrl))
                    {
                        try
                        {
                            var oldImagePath = currentImageUrl.Substring(currentImageUrl.IndexOf("/uploads/events/"));
                            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldImagePath);
                            if (System.IO.File.Exists(fullPath))
                                System.IO.File.Delete(fullPath);
                        }
                        catch { }
                    }
                    var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
                    var fileExtension = Path.GetExtension(image.FileName);
                    var imageFileName = $"event_{timestamp}{fileExtension}";
                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "events");
                    if (!Directory.Exists(uploadsDir))
                        Directory.CreateDirectory(uploadsDir);
                    var filePath = Path.Combine(uploadsDir, imageFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    return $"{requestScheme}://{requestHost}/uploads/events/{imageFileName}";
                }

                var updatedEvent = await _eventsService.UpdateEventAsync(id, eventDto, ImageHandler, requestScheme, requestHost);
                if (updatedEvent == null)
                    return NotFound(new { message = $"Event with ID: {id} not found" });

                return Ok(new { success = true, event_ = updatedEvent });
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
                async Task ImageDeleter(string? imageUrl)
                {
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        try
                        {
                            var imagePath = imageUrl.Substring(imageUrl.IndexOf("/uploads/events/"));
                            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath);
                            if (System.IO.File.Exists(fullPath))
                                System.IO.File.Delete(fullPath);
                        }
                        catch { }
                    }
                }

                var deleted = await _eventsService.DeleteEventAsync(id, ImageDeleter);
                if (!deleted)
                    return NotFound(new { message = $"Event with ID: {id} not found" });

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
                var events = await _eventsService.GetEventsByCategoryAsync(category);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching events by category: {Category}", category);
                return StatusCode(500, new { message = $"An error occurred while retrieving events for category: {category}", error = ex.Message });
            }
        }
    }
}