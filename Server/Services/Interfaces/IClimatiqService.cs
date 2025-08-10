using System.Threading.Tasks;

namespace Server.Services.Interfaces
{
    public interface IClimatiqService
    {
        Task<double> CalculateCo2Async(string category, double value);
        string GetMatchedCategory(string rawCategory); // ← Add this line
    }
}
