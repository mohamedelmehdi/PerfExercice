// Copyright (C) Abc Arbitrage Asset Management - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Written by Olivier Coanet <o.coanet@abc-arbitrage.com>, 2020-10-06

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

// TODO: Make the existing tests pass

namespace AbcArbitrage.Homework.Routing
{
    public class MessageQueueTests
    {
        [Fact]
        public void ShouldDequeueSingleMessage()
        {
            // Arrange
            var queue = new MessageQueue();
            var clientId = new ClientId("Client.1");
            var message = new RoutableMessages.InstrumentAdded();
            queue.EnqueueForClient(clientId, message);

            // Act
            var result = queue.TryDequeueForClient(clientId, out var dequeue);

            // Assert
            Assert.True(result);
            Assert.Same(message, dequeue);
        }

        [Fact]
        public void ShouldTryDequeueEmptyQueue()
        {
            // Arrange
            var queue = new MessageQueue();
            var clientId = new ClientId("Client.1");

            // Act
            var result = queue.TryDequeueForClient(clientId, out var dequeue);

            // Assert
            Assert.False(result);
            Assert.Null(dequeue);
        }

        [Fact]
        public void ShouldDequeueSingleMessageForClient()
        {
            // Arrange
            var queue = new MessageQueue();
            var clientId1 = new ClientId("Client.1");
            var message1 = new RoutableMessages.InstrumentAdded();
            queue.EnqueueForClient(clientId1, message1);

            var clientId2 = new ClientId("Client.2");
            var message2 = new RoutableMessages.InstrumentAdded();
            queue.EnqueueForClient(clientId2, message2);

            // Act
            var result = queue.TryDequeueForClient(clientId2, out var dequeue);

            // Assert
            Assert.True(result);
            Assert.Same(message2, dequeue);
        }

        [Fact]
        public void ShouldTryDequeueEmptyQueueForClient()
        {
            // Arrange
            var queue = new MessageQueue();
            var clientId1 = new ClientId("Client.1");
            var message1 = new RoutableMessages.InstrumentAdded();
            queue.EnqueueForClient(clientId1, message1);

            var clientId2 = new ClientId("Client.2");

            // Act
            var result = queue.TryDequeueForClient(clientId2, out var dequeue);

            // Assert
            Assert.False(result);
            Assert.Null(dequeue);
        }

        [Theory]
        [InlineData(MessagePriority.Low)]
        [InlineData(MessagePriority.Normal)]
        [InlineData(MessagePriority.High)]
        public void ShouldDequeueInOrderForSamePriority(MessagePriority priority)
        {
            // Arrange
            var queue = new MessageQueue();
            var clientId = new ClientId("Client.1");
            var message1 = new RoutableMessages.InstrumentAdded();
            var message2 = new RoutableMessages.InstrumentAdded();
            queue.EnqueueForClient(clientId, message1, priority);
            queue.EnqueueForClient(clientId, message2, priority);

            // Act
            queue.TryDequeueForClient(clientId, out var dequeue1);
            queue.TryDequeueForClient(clientId, out var dequeue2);

            // Assert
            Assert.Same(message1, dequeue1);
            Assert.Same(message2, dequeue2);
        }

        [Fact]
        public void ShouldDequeueHighPriorityMessageFirst()
        {
            // Arrange
            var queue = new MessageQueue();
            var clientId = new ClientId("Client.1");
            var message1 = new RoutableMessages.InstrumentAdded();
            var message2 = new RoutableMessages.InstrumentAdded();
            var message3 = new RoutableMessages.InstrumentAdded();
            queue.EnqueueForClient(clientId, message1, MessagePriority.Low);
            queue.EnqueueForClient(clientId, message2, MessagePriority.Normal);
            queue.EnqueueForClient(clientId, message3, MessagePriority.High);

            // Act
            queue.TryDequeueForClient(clientId, out var dequeue1);
            queue.TryDequeueForClient(clientId, out var dequeue2);
            queue.TryDequeueForClient(clientId, out var dequeue3);

            // Assert
            Assert.Same(message3, dequeue1);
            Assert.Same(message2, dequeue2);
            Assert.Same(message1, dequeue3);
        }

        [Fact]
        public void ShouldWriteAndReadFromMultipleThreads()
        {
            const int clientCount = 10_00;
            var queue = new MessageQueue();
            var clients = Enumerable.Range(1, clientCount).Select(x => new ClientId($"Client.{x}")).ToArray();

            const int messageCount = 10_000_000;
            Parallel.For(0,
                         messageCount,
                         i => queue.EnqueueForClient(clients[i % clientCount], new RoutableMessages.InstrumentAdded(), (MessagePriority)(i % 3)));

            Parallel.For(0,
                         messageCount,
                         i => Assert.True(queue.TryDequeueForClient(clients[i % clientCount], out _)));

            // Queue should be empty
            foreach (var clientId in clients)
            {
                Assert.False(queue.TryDequeueForClient(clientId, out _));
            }
        }

        [Fact]
        public void ShouldReadFromMultipleThreads()
        {
            var queue = new MessageQueue();
            var clientId = new ClientId("Client");
            const int messageCount = 1_000_000;
            for (var i = 0; i < messageCount; i++)
            {
                queue.EnqueueForClient(clientId, new SimpleMessages.ExchangeAdded());
            }

            Parallel.For(0, messageCount, _ => Assert.True(queue.TryDequeueForClient(clientId, out var _)));

            // Queue should be empty
            Assert.False(queue.TryDequeueForClient(clientId, out _));
        }

        [Fact]
        public void ShouldPreserveOrderForElementsOfEqualPriority()
        {
            // Arrange
            var queue = new MessageQueue();
            var clientId = new ClientId("Client.1");

            queue.EnqueueForClient(clientId, new SimpleMessages.ExchangeAdded { ExchangeId = 101 }, MessagePriority.Low);
            queue.EnqueueForClient(clientId, new SimpleMessages.ExchangeAdded { ExchangeId = 301 }, MessagePriority.High);
            queue.EnqueueForClient(clientId, new SimpleMessages.ExchangeAdded { ExchangeId = 102 }, MessagePriority.Low);
            queue.EnqueueForClient(clientId, new SimpleMessages.ExchangeAdded { ExchangeId = 201 }, MessagePriority.Normal);
            queue.EnqueueForClient(clientId, new SimpleMessages.ExchangeAdded { ExchangeId = 202 }, MessagePriority.Normal);
            queue.EnqueueForClient(clientId, new SimpleMessages.ExchangeAdded { ExchangeId = 302 }, MessagePriority.High);

            var exchangeIds = new List<int>();

            // Act
            while (queue.TryDequeueForClient(clientId, out var message))
            {
                exchangeIds.Add(((SimpleMessages.ExchangeAdded)message).ExchangeId);
            }

            // Assert
            var expectedExchangeIds = new[] { 301, 302, 201, 202, 101, 102 };
            Assert.Equal(expectedExchangeIds, exchangeIds);
        }
    }
}
