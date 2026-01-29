using MediatR;

namespace Datahub.Infrastructure.Queues.Messages;

public record AntivirusPostScanMessage(
    DateTime Timestamp,
    AntivirusScanStatus Result,
    string WorkspaceAcronym,
    string UploadUser,
    string UploadBatchId
) : IRequest;
