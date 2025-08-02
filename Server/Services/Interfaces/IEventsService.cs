using Server.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.DTOs;

namespace Server.Services.Interfaces
{
    public interface IEventsService
    {
        Task<IEnumerable<object>> GetEventsAsync();
        Task<object?> GetEventByIdAsync(int id);
        Task<IEnumerable<object>> GetUpcomingEventsAsync();
        Task<object> CreateEventAsync(object eventDto, Func<IFormFile?, Task<string?>> imageHandler, string requestScheme, string requestHost);
        Task<object?> UpdateEventAsync(int id, object eventDto, Func<IFormFile?, string?, Task<string?>> imageHandler, string requestScheme, string requestHost);
        Task<bool> DeleteEventAsync(int id, Func<string?, Task> imageDeleter);
        Task<IEnumerable<object>> GetEventsByCategoryAsync(string category);
    }
}