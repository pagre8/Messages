using Cassandra;
using Moq;
using RabbitMQ.Client;
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
        private readonly Mock<ICassandraAccess> _mockCassandraAccess;
        private readonly Mock<ISession> _mockSession;
        private readonly Mock<IModel> _mockChannel;

        private readonly Mock<IRabbitAccess> _mockRabbitAccess;
        private readonly Mock<MessageService> _mockMessageService;

        public MessageServiceTests()
        {
            _mockCassandraAccess = new Mock<ICassandraAccess>();
            _mockSession = new Mock<ISession>();

            _mockCassandraAccess.Setup(s => s.Session).Returns(_mockSession.Object);

            _mockRabbitAccess = new Mock<IRabbitAccess>();
            _mockChannel = new Mock<IModel>();

            _mockRabbitAccess.Setup(r=>r.CreateModel()).Returns(_mockChannel.Object);

            _mockMessageService = new Mock<MessageService>(_mockCassandraAccess.Object, _mockRabbitAccess.Object);
        }

        #region PublishToRabbit Tests

        [Fact]
        public void PublishToRabbit_ShouldThrowException_WhenInvalidDataIsProvided()
        {
            var invalidMessageData = new MessageData
            {
                Id = Guid.NewGuid(),
                IdChat = Guid.Empty,
                IdSender = Guid.NewGuid(),
                Content = null,
                CreatedAt = DateTime.UtcNow
            };
            Assert.Throws<ArgumentException>(() => _mockMessageService.Object.PublishToRabbit(_mockChannel.Object, invalidMessageData));

        }

        #endregion

        #region SaveToCassandra Tests
        [Fact]
        public async void SaveToCassandra_ShouldExecuteCommand_WhenValidDataIsProvided()
        {
            var preparedCommand = new Mock<PreparedStatement>();
            var messageData = new MessageData
            {
                Id = Guid.NewGuid(),
                IdChat = Guid.NewGuid(),
                IdSender = Guid.NewGuid(),
                Content = "Test Message",
                CreatedAt = DateTime.UtcNow
            };

            var mockRowSet = new Mock<RowSet>();

            _mockSession.Setup(s => s.ExecuteAsync(It.IsAny<BoundStatement>())).ReturnsAsync(mockRowSet.Object);

            await _mockMessageService.Object.SaveToCassandra(preparedCommand.Object, messageData);

            _mockSession.Verify(s => s.ExecuteAsync(It.IsAny<BoundStatement>()), Times.Once);
        }

        [Fact]
        public async Task SaveToCassandra_ShouldLogError_WhenExecutionFails() 
        {

            var preparedCommand = new Mock<PreparedStatement>();
            var messageData = new MessageData
            {
                Id = Guid.NewGuid(),
                IdChat = Guid.NewGuid(),
                IdSender = Guid.NewGuid(),
                Content = "Test Message",
                CreatedAt = DateTime.UtcNow
            };



            _mockSession.Setup(s => s.ExecuteAsync(It.IsAny<BoundStatement>())).ThrowsAsync(new Exception("Cassandra Error"));

            await _mockMessageService.Object.SaveToCassandra(preparedCommand.Object,messageData);

        }

        [Fact]
        public async Task SaveToCassandra_ShouldThrowException_WhenInvalidDataIsProvided()
        {
            var preparedCommand = new Mock<PreparedStatement>();
            var invalidMessageData = new MessageData
            {
                Id = Guid.NewGuid(),
                IdChat = Guid.Empty,
                IdSender = Guid.NewGuid(),
                Content = null,
                CreatedAt = DateTime.UtcNow
            };

            var mockRowSet = new Mock<RowSet>();

            _mockSession.Setup(s => s.ExecuteAsync(It.IsAny<BoundStatement>())).ReturnsAsync(mockRowSet.Object);

            await Assert.ThrowsAsync<ArgumentException>(async () => await _mockMessageService.Object.SaveToCassandra(preparedCommand.Object, invalidMessageData));

            
        }


        #endregion

        #region CheckAccess Tests
        [Fact]
        public async Task CheckAccess_ShouldReturnTrue_WhenResultsAreFound()
        {
            var preparedCommand = new Mock<PreparedStatement>();
            var mockRowSet = new Mock<RowSet>();
            var rowList = new List<Row> {new Mock<Row>().Object };

            mockRowSet.As<IEnumerable<Row>>().Setup(m => m.GetEnumerator()).Returns(rowList.GetEnumerator());

            _mockCassandraAccess.Setup(x => x.Session.PrepareAsync(It.IsAny<string>())).ReturnsAsync(preparedCommand.Object);

            _mockCassandraAccess.Setup(x => x.Session.ExecuteAsync(It.IsAny<BoundStatement>())).ReturnsAsync(mockRowSet.Object);

            var result = await _mockMessageService.Object.CheckAccess(Guid.NewGuid(),Guid.NewGuid());

            Assert.True(result );
        }

        [Fact]

        public async Task CheckAccess_ShouldReturnFalse_WhenResultsAreNotFound()
        {
            var preparedCommand = new Mock<PreparedStatement>();
            var mockRowSet = new Mock<RowSet>();
            var rowList = new List<Row>();

            mockRowSet.As<IEnumerable<Row>>().Setup(m => m.GetEnumerator()).Returns(rowList.GetEnumerator());

            _mockCassandraAccess.Setup(x => x.Session.PrepareAsync(It.IsAny<string>())).ReturnsAsync(preparedCommand.Object);

            _mockCassandraAccess.Setup(x => x.Session.ExecuteAsync(It.IsAny<BoundStatement>())).ReturnsAsync(mockRowSet.Object);

            var result = await _mockMessageService.Object.CheckAccess(Guid.NewGuid(), Guid.NewGuid());

            Assert.False(result);
        }

        [Fact]
        public async Task CheckAccess_ShouldReturnFalse_WhenExceptionIsThrown()
        {
            _mockCassandraAccess.Setup(x => x.Session.PrepareAsync(It.IsAny<string>())).ThrowsAsync(new Exception("Cassandra error"));

            var result = await _mockMessageService.Object.CheckAccess(Guid.NewGuid(), Guid.NewGuid());

            Assert.False(result);
        }
        #endregion


    }
}