namespace Datahub.Application.Services;

public interface IUserEnrollmentService
{
    public Task<string> SendUserDatahubPortalInvite(string registrationRequestEmail);
}