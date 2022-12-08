using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public interface IUserCircuitCounterService
    {
        Task<bool> IsSessionEnabled();
        int GetSessionCount();
    }
}