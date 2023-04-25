using System.Collections.Generic;
using System.Threading.Tasks;

namespace Datahub.Core.Services.Achievements;

public interface IPortalUserService
{
    Task<List<string>> GetUserAchivements();
    Task AddUserAchivements(IEnumerable<string> achivements);
}
