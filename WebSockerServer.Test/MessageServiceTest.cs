using Cassandra;
using Moq;
using RabbitMQ.Client;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using WebSocket_Server.DataAccess;
using WebSocket_Server.Infrastructure.Messaging;
using WebSocket_Server.Infrastructure.Services;
using WebSocket_Server.Interfaces;
using WebSocket_Server.Models;

namespace WebSockerServer.Test
{
    public class MessageServiceTests
    {
        private readonly Mock<CassandraAccess> _mockCassandraAccess;
        private readonly Mock<RabbitAccess> _mockRabbitAccess;
        private readonly MessageService _messageService;


        public MessageServiceTests()
        {
            _mockCassandraAccess = new Mock<CassandraAccess>();
            _mockRabbitAccess = new Mock<RabbitAccess>();

            _messageService = new MessageService(_mockCassandraAccess.Object, _mockRabbitAccess.Object);
        }


        [Fact]
        public async Task HandleWebSocketCommunication_ShouldPublishmessages_WhenAccessIsGranted()
        {
            var mockWebSocket = new Mock<WebSocket>();
            var messageData = new MessageData
            {
                Id = Guid.NewGuid(),
                IdChat = Guid.NewGuid(),
                IdSender = Guid.NewGuid(),
                Content = "Test Message",
                CreatedAt = DateTime.UtcNow
            };

            var mockResultSet = new Mock<RowSet>();
            var mockRow = new Mock<Row>();
            mockResultSet.Setup(rs => rs.FirstOrDefault()).Returns(mockRow.Object);
            _mockCassandraAccess.Setup(c => c._session.ExecuteAsync(It.IsAny<BoundStatement>()))
                .ReturnsAsync(mockResultSet.Object);

            var channelMock = new Mock<IModel>();
            _mockRabbitAccess.Setup(r => r._connection.CreateModel()).Returns(channelMock.Object);

            var buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(messageData));
            var webSocketReciveResult = new WebSocketReceiveResult(buffer.Length, WebSocketMessageType.Text, false);
            mockWebSocket.Setup(ws => ws.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(webSocketReciveResult);


            await _messageService.HandleWebSocketCommunication(mockWebSocket.Object);


            channelMock.Verify(c => c.BasicPublish(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IBasicProperties>(),
                It.IsAny<byte[]>()
            ), Times.Once);

        }


    }
}