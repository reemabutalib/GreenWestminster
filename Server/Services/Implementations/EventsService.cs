using Server.Data;
using Server.Models;
using Microsoft.EntityFrameworkCore;
using Server.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Server.DTOs;

namespace Server.Services.Implementations
{
    public class EventsService : IEventsService
    {
        private readonly AppDbContext _context;

        public EventsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>> GetEventsAsync()
        {
            return await _context.SustainabilityEvents
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
        }

        public async Task<object?> GetEventByIdAsync(int id)
        {
            return await _context.SustainabilityEvents
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
        }

        public async Task<IEnumerable<object>> GetUpcomingEventsAsync()
        {
            var today = DateTime.UtcNow;
            return await _context.SustainabilityEvents
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
        }

        public async Task<object> CreateEventAsync(object eventDto, Func<IFormFile?, Task<string?>> imageHandler, string requestScheme, string requestHost)
        {
            // eventDto should be cast to the correct DTO type in the controller before calling this method
            dynamic dto = eventDto;
            string? imageUrl = await imageHandler(dto.Image);

            var newEvent = new SustainabilityEvent
            {
                Title = dto.Title,
                Description = dto.Description,
                Location = dto.Location,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                RegistrationLink = dto.RegistrationLink,
                ImageUrl = imageUrl,
                Organizer = dto.Organizer,
                MaxAttendees = dto.MaxAttendees,
                Category = dto.Category,
                IsVirtual = dto.IsVirtual,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.SustainabilityEvents.Add(newEvent);
            await _context.SaveChangesAsync();

            return new
            {
                id = newEvent.Id,
                title = newEvent.Title,
                description = newEvent.Description,
                location = newEvent.Location,
                startDate = newEvent.StartDate,
                endDate = newEvent.EndDate,
                registrationLink = newEvent.RegistrationLink,
                imageUrl = newEvent.ImageUrl,
                organizer = newEvent.Organizer,
                maxAttendees = newEvent.MaxAttendees,
                category = newEvent.Category,
                isVirtual = newEvent.IsVirtual,
                createdAt = newEvent.CreatedAt,
                updatedAt = newEvent.UpdatedAt
            };
        }

        public async Task<object?> UpdateEventAsync(int id, object eventDto, Func<IFormFile?, string?, Task<string?>> imageHandler, string requestScheme, string requestHost)
        {
            dynamic dto = eventDto;
            var existingEvent = await _context.SustainabilityEvents.FindAsync(id);
            if (existingEvent == null) return null;

            string? imageUrl = existingEvent.ImageUrl;
            if (dto.Image != null && dto.Image.Length > 0)
            {
                imageUrl = await imageHandler(dto.Image, existingEvent.ImageUrl);
            }

            existingEvent.Title = dto.Title;
            existingEvent.Description = dto.Description;
            existingEvent.Location = dto.Location;
            existingEvent.StartDate = dto.StartDate;
            existingEvent.EndDate = dto.EndDate;
            existingEvent.RegistrationLink = dto.RegistrationLink;
            existingEvent.ImageUrl = imageUrl;
            existingEvent.Organizer = dto.Organizer;
            existingEvent.MaxAttendees = dto.MaxAttendees;
            existingEvent.Category = dto.Category;
            existingEvent.IsVirtual = dto.IsVirtual;
            existingEvent.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new
            {
                id = existingEvent.Id,
                title = existingEvent.Title,
                description = existingEvent.Description,
                location = existingEvent.Location,
                startDate = existingEvent.StartDate,
                endDate = existingEvent.EndDate,
                registrationLink = existingEvent.RegistrationLink,
                imageUrl = existingEvent.ImageUrl,
                organizer = existingEvent.Organizer,
                maxAttendees = existingEvent.MaxAttendees,
                category = existingEvent.Category,
                isVirtual = existingEvent.IsVirtual,
                updatedAt = existingEvent.UpdatedAt
            };
        }

        public async Task<bool> DeleteEventAsync(int id, Func<string?, Task> imageDeleter)
        {
            var existingEvent = await _context.SustainabilityEvents.FindAsync(id);
            if (existingEvent == null) return false;

            await imageDeleter(existingEvent.ImageUrl);

            _context.SustainabilityEvents.Remove(existingEvent);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<object>> GetEventsByCategoryAsync(string category)
        {
            return await _context.SustainabilityEvents
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
        }
    }
}