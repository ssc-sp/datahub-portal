namespace Datahub.Application.Services.UserManagement
{
    public interface ICultureService
    {
        public const string French = "fr";
        public const string English = "en";

        public string Culture { get; }
        public bool IsEnglish => Culture == English;
        public bool IsFrench => Culture == French;
    }
}
