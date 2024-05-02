using Datahub.Application.Services;
using Datahub.Core.Services;

namespace Datahub.Infrastructure.Offline;

public class OfflineDatahubTelemetryAuditingService : IDatahubAuditingService 
{
	public Task TrackAdminEvent(string scope, string source, AuditChangeType changeType, params (string Key, string Value)[] details)
	{
		return Task.CompletedTask;
	}

	public Task TrackDataEvent(string objectId, string table, AuditChangeType changeType, bool anonymous, params (string Key, string Value)[] details)
	{
		return Task.CompletedTask;
	}

	public Task TrackException(Exception exception, params (string Key, string Value)[] details)
	{
		return Task.CompletedTask;
	}

	public Task TrackSecurityEvent(string scope, string table, AuditChangeType changeType, params (string Key, string Value)[] details)
	{
		return Task.CompletedTask;
	}

	public Task TrackEvent(string message, params (string Key, string Value)[] details)
	{
		return Task.CompletedTask;
	}
}