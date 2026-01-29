namespace Datahub.Infrastructure.Queues.Messages;

public enum AntivirusScanStatus
{
    Unscanned,
    Scanning,
    Success,
    Virus,
    ScanError
}
