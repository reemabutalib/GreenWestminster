using Server.Models;
using Server.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Server.DTOs;
using Server.Repositories;

namespace Server.Services.Implementations
{
    public class EventsService : IEventsService
    {
        private readonly SustainableEventRepository _eventRepository;

        public EventsService(SustainableEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<IEnumerable<object>> GetEventsAsync()
        {
            var events = await _eventRepository.GetAllAsync();
            return events
                .OrderBy(e => e.StartDate)
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
                });
        }

        public async Task<object?> GetEventByIdAsync(int id)
        {
            var e = await _eventRepository.GetByIdAsync(id);
            if (e == null) return null;
            return new
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
            };
        }

        public async Task<IEnumerable<object>> GetUpcomingEventsAsync()
        {
            var today = DateTime.UtcNow;
            var events = await _eventRepository.GetAllAsync();
            return events
                .Where(e => e.StartDate > today)
                .OrderBy(e => e.StartDate)
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
                });
        }

        public async Task<object> CreateEventAsync(object eventDto, Func<IFormFile?, Task<string?>> imageHandler, string requestScheme, string requestHost)
        {
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

            await _eventRepository.AddAsync(newEvent);

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
            var existingEvent = await _eventRepository.GetByIdAsync(id);
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

            await _eventRepository.UpdateAsync(existingEvent);

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
            var existingEvent = await _eventRepository.GetByIdAsync(id);
            if (existingEvent == null) return false;

            await imageDeleter(existingEvent.ImageUrl);

            await _eventRepository.DeleteAsync(id);

            return true;
        }

        public async Task<IEnumerable<object>> GetEventsByCategoryAsync(string category)
        {
            var events = await _eventRepository.GetAllAsync();
            return events
                .Where(e => e.Category != null && e.Category.Contains(category, StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.StartDate)
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
                });
        }
    }
}