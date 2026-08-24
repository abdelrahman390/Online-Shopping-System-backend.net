using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;

namespace Online_Shopping_SystemTests.Services
{

    public class EmailServiceTests
    {
        [Fact]
        public async Task SendEmailAsync_ShouldSendEmailSuccessfully()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK
                });

            var httpClient = new HttpClient(handlerMock.Object);

            
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Mailtrap:ApiToken"] = "test-token"
                })
                .Build();

            var service = new EmailService(
                httpClient,
                configuration
            );

            // Act
            await service.SendEmailAsync(
                "test@example.com",
                "Test Subject",
                "Hello from test"
            );

            // Assert
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(request =>
                    request.Method == HttpMethod.Post &&
                    request.RequestUri!.ToString() ==
                        "https://send.api.mailtrap.io/api/send"
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}