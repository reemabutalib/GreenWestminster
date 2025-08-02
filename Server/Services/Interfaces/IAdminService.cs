using System.Threading.Tasks;
using Server.Models;
using Server.DTOs;

namespace Server.Services.Interfaces
{
    public interface IAdminService
    {
        Task<object> GetUserStatsAsync();
        Task<object> GetActivityCompletionsAsync(DateTime? startDate = null, DateTime? endDate = null);
        
    }
}