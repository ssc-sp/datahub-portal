using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Datahub.Functions.Entities
{
    public class TeamsWebhookMessage
    {
        public TeamsWebhookMessage(string title, string reportType)
        {
            Summary = $"{title} - {reportType}";
            Sections.Add(new TeamsWebhookSection(title, reportType));
        }

        [JsonPropertyName("@type")]
        public string MessageType { get; private set; } = "AdaptiveCard";

        [JsonPropertyName("@context")]
        public string MessageContext { get; private set; } = "http://schema.org/extensions";

        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("sections")]
        public IList<TeamsWebhookSection> Sections { get; private set; } = [];
    }

    public class TeamsWebhookSection(string title, string subtitle)
    {
        [JsonPropertyName("activityTitle")]
        public string Title { get; set; } = title;

        [JsonPropertyName("activitySubtitle")]
        public string Subtitle { get; set; } = subtitle;

        [JsonPropertyName("facts")]
        public IList<TeamsWebhookFact> Facts { get; set; } = [];

        public void AddFact(string name, string value) => Facts.Add(new TeamsWebhookFact(name, value));

        public void AddNonEmptyFact(string name, string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                AddFact(name, value);
            }
        }
    }
    
    public record TeamsWebhookFact(string Name, string Value);
}
