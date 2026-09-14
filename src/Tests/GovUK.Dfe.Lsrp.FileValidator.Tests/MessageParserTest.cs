using GovUK.Dfe.Lsrp.FileValidator.Models;
using Xunit.Abstractions;

namespace GovUK.Dfe.Lsrp.FileValidator.Tests;

public class MessageParserTest(ITestOutputHelper output)
{
    [Fact]
    public void Parse_Valid_Message()
    {
        const string laCode = "LA-Code";
        const string laName = "LA-Name";

        // Arrange
        FileUploadedMessage message = new()
        {
            MessageId = "msg001",
            Message = new Message
            {
                Metadata = new Metadata
                {
                    ApplicationId = "app001",
                    ApplicationReference = "ref001",
                },
                Payload = new Payload
                {
                    FileUri = "https://example.com/file.csv",
                    FileId = "file001",
                    LocalAuthority = $"{{\"name\":\"{laName}\",\"code\":\"{laCode}\"}}"
                }
            }
        };

        // Act
        List<string> errors = [];
        bool result = MessageParser.Parse(message, out MessageData? messageData, errors);

        // Assert
        Assert.True(result);
        Assert.Empty(errors);
        Assert.NotNull(messageData);
        Assert.NotNull(messageData.LocalAuthority);
        Assert.Equal(laCode, messageData.LocalAuthority.Code);
        Assert.Equal(laName, messageData.LocalAuthority.Name);
        Assert.Equal($"{laCode} {laName}", messageData.LocalAuthority.ToString());
    }

    [Fact]
    public void Parse_WhenLocalAuthorityMissing_ReturnsFalse()
    {
        // Arrange
        FileUploadedMessage message = new()
        {
            MessageId = "msg001",
            Message = new Message
            {
                Metadata = new Metadata
                {
                    ApplicationId = "app001",
                    ApplicationReference = "ref001",
                },
                Payload = new Payload
                {
                    FileUri = "https://example.com/file.csv",
                    FileId = "file001",
                    LocalAuthority = null
                }
            }
        };

        // Act
        List<string> errors = [];
        bool result = MessageParser.Parse(message, out MessageData? messageData, errors);

        // Assert
        Assert.False(result);
        Assert.Null(messageData);
        output.WriteLine($"Errors: {string.Join(", ", errors)}");
        Assert.Contains("Local authority is missing or invalid.", errors);
    }

    [Fact]
    public void Parse_WhenLocalAuthorityIsInvalidJson_ReturnsFalse()
    {
        // Arrange
        FileUploadedMessage message = new()
        {
            MessageId = "msg001",
            Message = new Message
            {
                Metadata = new Metadata
                {
                    ApplicationId = "app001",
                    ApplicationReference = "ref001",
                },
                Payload = new Payload
                {
                    FileUri = "https://example.com/file.csv",
                    FileId = "file001",
                    LocalAuthority = "not-json"
                }
            }
        };

        // Act
        List<string> errors = [];
        bool result = MessageParser.Parse(message, out MessageData? messageData, errors);

        // Assert
        Assert.False(result);
        Assert.Null(messageData);
        output.WriteLine($"Errors: {string.Join(", ", errors)}");
        Assert.Contains("Local authority is missing or invalid.", errors);
    }

    [Fact]
    public void Parse_InvalidMessage_ReturnsFalse()
    {
        // Arrange
        FileUploadedMessage message = new()
        {
            MessageId = "msg001",
            Message = new Message()
        };

        // Act
        List<string> errors = [];
        bool result = MessageParser.Parse(message, out MessageData? messageData, errors);

        // Assert
        Assert.False(result);
        Assert.Null(messageData);
        output.WriteLine($"Errors: {string.Join(", ", errors)}");
        Assert.Contains("File information is missing or incomplete.", errors);
        Assert.Contains("Application information is missing or incomplete.", errors);
        Assert.Contains("Local authority is missing or invalid.", errors);
    }

    [Fact]
    public void Parse_NoMessage_ReturnsFalse()
    {
        // Arrange
        FileUploadedMessage message = new()
        {
            MessageId = "msg001"
        };

        // Act
        List<string> errors = [];
        bool result = MessageParser.Parse(message, out MessageData? messageData, errors);

        // Assert
        Assert.False(result);
        Assert.Null(messageData);
        output.WriteLine($"Errors: {string.Join(", ", errors)}");
        Assert.Contains("Message is null.", errors);
    }
}
