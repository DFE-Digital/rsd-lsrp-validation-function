using GovUK.Dfe.Lsrp.FileValidator.Models;

namespace GovUK.Dfe.Lsrp.FileValidator.Tests;

public class MessageParserTest
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
        MessageParser.Parse(message, out MessageData? messageData);

        // Assert
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
        bool result = MessageParser.Parse(message, out MessageData? messageData);

        // Assert
        Assert.False(result);
        Assert.Null(messageData);
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
        bool result = MessageParser.Parse(message, out MessageData? messageData);

        // Assert
        Assert.False(result);
        Assert.Null(messageData);
    }
}
