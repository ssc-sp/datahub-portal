using Datahub.Infrastructure.Queues.Messages;

namespace Datahub.Functions.Services
{
    public interface IEmailService
    {
        public EmailRequestMessage? BuildEmail(string template, List<string> sendTo, List<string> bccTo, Dictionary<string,string> bodyArgs, Dictionary<string, string> subjectArgs);
        public string PopulateTemplate(string template, Dictionary<string,string> args);
    }
}