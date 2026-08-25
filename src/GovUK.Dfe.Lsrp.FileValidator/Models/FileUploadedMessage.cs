using System.Text.Json.Serialization;

namespace GovUK.Dfe.Lsrp.FileValidator.Models;

public class FileUploadedMessage
{
    public string? MessageId { get; set; }
    public object? RequestId { get; set; }
    public object? CorrelationId { get; set; }
    public string? ConversationId { get; set; }
    public object? InitiatorId { get; set; }
    public string? SourceAddress { get; set; }
    public string? DestinationAddress { get; set; }
    public object? ResponseAddress { get; set; }
    public object? FaultAddress { get; set; }
    public string[]? MessageType { get; set; }
    public Message? Message { get; set; }
    public object? ExpirationTime { get; set; }
    public DateTime SentTime { get; set; }
    public Headers? Headers { get; set; }
    public Host? Host { get; set; }
}

public class Message
{
    public string? MessageType { get; set; }
    public string? Version { get; set; }
    public string? TopicName { get; set; }
    public Payload? Payload { get; set; }
    public Metadata? Metadata { get; set; }
}

public class Payload
{
    public string? FileUri { get; set; }
}

public class Metadata
{
    public string? ApplicationId { get; set; }
    public string? ApplicationReference { get; set; }
    public string? TemplateId { get; set; }
}

public class Headers
{
    public string? MessageType { get; set; }
    public string? EventKind { get; set; }
    public string? ServiceName { get; set; }
    public string? TenantId { get; set; }
    public string? TenantName { get; set; }
    public string? SchemaVersion { get; set; }
}

public class Host
{
    public string? MachineName { get; set; }
    public string? ProcessName { get; set; }
    public int ProcessId { get; set; }
    public string? Assembly { get; set; }
    public string? AssemblyVersion { get; set; }
    public string? FrameworkVersion { get; set; }
    public string? MassTransitVersion { get; set; }
    public string? OperatingSystemVersion { get; set; }
}
