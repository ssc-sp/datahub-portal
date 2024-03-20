using MediatR;
using System.Text.Json.Serialization;

namespace Datahub.Infrastructure.Queues.Messages;

public class EmailRequestMessage : IRequest
{
    [JsonPropertyName("to")]
    public required List<string> To { get; set; }
    [JsonPropertyName("subject")]
    public required string Subject { get; set; }
    [JsonPropertyName("body")]
    public required string? Body { get; set; }

    [JsonPropertyName("cc")]
    public List<string> CcTo { get; set; } = new();
    [JsonPropertyName("bcc")]
    public List<string> BccTo { get; set; } = new();
    [JsonPropertyName("attachements")]
    public List<MessageAttachment> Attachements { get; set; } = new();
    [JsonPropertyName("template")]
    public string? Template { get; set; }
    [JsonPropertyName("templateData")]
    public string? TemplateData { get; set; }

    public bool IsValid => To?.Count > 0;
}

public class MessageAttachment
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("data")]
    public string? Data { get; set; }
}