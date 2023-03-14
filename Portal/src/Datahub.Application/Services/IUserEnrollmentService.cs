namespace Datahub.Application.Services;

public interface IUserEnrollmentService
{
    public bool IsValidGcEmail(string? email);
    public Task<string> SendUserDatahubPortalInvite(string? registrationRequestEmail, string? inviterName);
}