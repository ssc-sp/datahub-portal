using Datahub.Infrastructure.Queues.Messages;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public EmailRequestMessage? BuildEmail(string template, List<string> sendTo, List<string> bccTo, Dictionary<string,string> bodyArgs, Dictionary<string, string> subjectArgs)
        {
            var (subject, body) = GetEmailTemplate(template);

            if (subject is null || body is null)
            {
                _logger.LogError("Email template file missing!");
                return null;
            }
            
            var populatedSubject = PopulateTemplate(subject, subjectArgs);
            var populatedBody = PopulateTemplate(body, bodyArgs);
            
            EmailRequestMessage notificationEmail = new()
            {
                To = sendTo,
                BccTo = bccTo,
                Subject = populatedSubject,
                Body = populatedBody,
                Template=template
            };
            return notificationEmail;
        }

        public string PopulateTemplate(string template, Dictionary<string, string> args)
        {
            foreach (var (key,value) in args)
            {
                template = template.Replace(key, value);
            }

            return template;
        }
        
        public static (string? Subject, string? Body) GetEmailTemplate(string fileName)
        {
            var filePath = $"./EmailTemplates/{fileName}";
            if (!File.Exists(filePath))
                return (null, null);

            var lines = File.ReadAllLines(filePath);
            if (lines is null) 
                return (null, null);

            var subject = lines.FirstOrDefault();
            var body = string.Join("\n", lines.Skip(1));

            return (subject, body);
        }
    }
}