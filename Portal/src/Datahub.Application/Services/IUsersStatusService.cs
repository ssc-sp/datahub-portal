namespace Datahub.Application.Services
{
    public interface IUsersStatusService
    {
        public Task<Dictionary<string, List<string>>?> GetUsersStatus();
    }
}