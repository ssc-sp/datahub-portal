using System;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public class UserCircuitCounterService : IDisposable, IUserCircuitCounterService
    {
        readonly IGlobalSessionManager _sessionManager;
        readonly IUserInformationService _userInformationService;
        private string _sessionId;
        private bool? _enabled;

        public UserCircuitCounterService(IGlobalSessionManager sessionManager, IUserInformationService userInformationService)
        {
            _sessionManager = sessionManager;
            _userInformationService = userInformationService;
            _enabled = null;
        }

        public async Task<bool> IsSessionEnabled()
        {
            if (_enabled is null)
            {
                _sessionId = await _userInformationService.GetUserIdString();
                _enabled = _sessionManager.TryAddSession(_sessionId);
            }
            return _enabled.Value;
        }

        public int GetSessionCount()
        {
            return _sessionManager.GetSessionCount(_sessionId);
        }

        public void Dispose()
        {
            if (_enabled == true)
            {
                _sessionManager.RemoveSession(_sessionId);
                _sessionId = null;
            }                
        }
    }
}
