// Copyright (C) Abc Arbitrage Asset Management - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Written by Olivier Coanet <o.coanet@abc-arbitrage.com>, 2020-10-06

using System.Collections.Generic;
using System.Linq;
using Xunit;

// TODO: Make the existing tests pass
// TODO: Add missing matching tests
// TODO: Add subscription removal tests

namespace AbcArbitrage.Homework.Routing
{
    public class MessageRouterTests
    {
        private readonly SubscriptionIndex _subscriptionIndex;
        private readonly MessageRouter _router;

        public MessageRouterTests()
        {
            _subscriptionIndex = new SubscriptionIndex();
            _router = new MessageRouter(_subscriptionIndex);
        }

        [Fact]
        public void ShouldIncludeSingleMatchingSubscription()
        {
            // Arrange
            var clientId = new ClientId("Client.1");
            _subscriptionIndex.AddSubscriptions(new[]
            {
                Subscription.Of<SimpleMessages.ExchangeAdded>(clientId),
            });

            // Act
            var clientIds = _router.GetConsumers(new SimpleMessages.ExchangeAdded()).ToList();

            // Assert
            Assert.Equal(new[] { clientId }, clientIds);
        }

        [Fact]
        public void ShouldIncludeMatchingClientForTwoMessages()
        {
            // Arrange
            var clientId = new ClientId("Client.1");
            _subscriptionIndex.AddSubscriptions(new[]
            {
                Subscription.Of<SimpleMessages.ExchangeAdded>(clientId),
                Subscription.Of<SimpleMessages.ExchangeTradingPhaseChanged>(clientId),
            });

            // Act
            var clientIdsForMessage1 = _router.GetConsumers(new SimpleMessages.ExchangeAdded()).ToList();
            var clientIdsForMessage2 = _router.GetConsumers(new SimpleMessages.ExchangeTradingPhaseChanged()).ToList();

            // Assert
            Assert.Equal(new[] { clientId }, clientIdsForMessage1);
            Assert.Equal(new[] { clientId }, clientIdsForMessage2);
        }

        [Fact]
        public void ShouldExcludeSubscriptionWithOtherMessageType()
        {
            // Arrange
            var clientId1 = new ClientId("Client.1");
            var clientId2 = new ClientId("Client.2");
            _subscriptionIndex.AddSubscriptions(new[]
            {
                Subscription.Of<SimpleMessages.ExchangeAdded>(clientId1),
                Subscription.Of<SimpleMessages.ExchangeTradingPhaseChanged>(clientId2),
            });

            // Act
            var clientIds = _router.GetConsumers(new SimpleMessages.ExchangeAdded()).ToList();

            // Assert
            Assert.Equal(new[] { clientId1 }, clientIds);
        }

        [Fact]
        public void ShouldExcludeSingleSubscriptionWithOtherMessageType()
        {
            // Arrange
            var clientId = new ClientId("Client.1");
            _subscriptionIndex.AddSubscriptions(new[]
            {
                Subscription.Of<SimpleMessages.ExchangeAdded>(clientId),
            });

            // Act
            var clientIds = _router.GetConsumers(new SimpleMessages.ExchangeTradingPhaseChanged()).ToList();

            // Assert
            Assert.Empty(clientIds);
        }

        [Fact]
        public void ShouldIncludeSingleMatchingRoutableSubscription()
        {
            // Arrange
            var clientId = new ClientId("Client.1");
            _subscriptionIndex.AddSubscriptions(new[]
            {
                Subscription.Of<RoutableMessages.PriceUpdated>(clientId, new ContentPattern("NASDAQ")),
            });

            var routableMessage = new RoutableMessages.PriceUpdated { ExchangeCode = "NASDAQ", Symbol = "MSFT" };

            // Act
            var clientIds = _router.GetConsumers(routableMessage).ToList();

            // Assert
            Assert.Equal(new[] { clientId }, clientIds);
        }

        [Fact]
        public void ShouldIncludeRoutableSubscriptionsForTwoClients()
        {
            // Arrange
            var clientId1 = new ClientId("Client.1");
            var clientId2 = new ClientId("Client.2");
            var clientId3 = new ClientId("Client.3");

            _subscriptionIndex.AddSubscriptions(new[]
            {
                Subscription.Of<RoutableMessages.PriceUpdated>(clientId1, new ContentPattern("NASDAQ", "*")),
                Subscription.Of<RoutableMessages.PriceUpdated>(clientId2, new ContentPattern("NYSE", "*")),
                Subscription.Of<RoutableMessages.PriceUpdated>(clientId3, new ContentPattern("NASDAQ", "*")),
            });

            var routableMessage = new RoutableMessages.PriceUpdated { ExchangeCode = "NASDAQ", Symbol = "MSFT" };

            // Act
            var clientIds = _router.GetConsumers(routableMessage).ToList();

            // Assert
            Assert.Equal(new HashSet<ClientId> { clientId1, clientId3 }, new HashSet<ClientId>(clientIds));
        }

        [Fact]
        public void ShouldIncludeMatchingRoutableSubscription()
        {
            // Arrange
            var clientId1 = new ClientId("Client.1");
            var clientId2 = new ClientId("Client.2");
            _subscriptionIndex.AddSubscriptions(new[]
            {
                Subscription.Of<RoutableMessages.PriceUpdated>(clientId1, new ContentPattern("NASDAQ")),
                Subscription.Of<RoutableMessages.PriceUpdated>(clientId2, new ContentPattern("NYSE")),
            });

            var routableMessage = new RoutableMessages.PriceUpdated { ExchangeCode = "NASDAQ", Symbol = "MSFT" };

            // Act
            var clientIds = _router.GetConsumers(routableMessage).ToList();

            // Assert
            Assert.Equal(new[] { clientId1 }, clientIds);
        }

        [Fact]
        public void ShouldExcludeRoutableSubscriptionWithOtherMessageType()
        {
            // Arrange
            var clientId = new ClientId("Client.1");
            _subscriptionIndex.AddSubscriptions(new[]
            {
                Subscription.Of<RoutableMessages.InstrumentAdded>(clientId, new ContentPattern("9")),
            });

            var routableMessage = new RoutableMessages.InstrumentDelisted { ExchangeId = 9 };

            // Act
            var clientIds = _router.GetConsumers(routableMessage).ToList();

            // Assert
            Assert.Empty(clientIds);
        }

        [Fact]
        public void ShouldExcludeRoutableSubscriptionWithOtherContent()
        {
            // Arrange
            var clientId = new ClientId("Client.1");
            _subscriptionIndex.AddSubscriptions(new[]
            {
                Subscription.Of<RoutableMessages.PriceUpdated>(clientId, new ContentPattern("NASDAQ")),
            });

            var routableMessage = new RoutableMessages.PriceUpdated { ExchangeCode = "NYSE" };

            // Act
            var clientIds = _router.GetConsumers(routableMessage).ToList();

            // Assert
            Assert.Empty(clientIds);
        }

        [Theory]
        [InlineData("NASDAQ")]
        [InlineData("NASDAQ.MSFT")]
        [InlineData("*")]
        [InlineData("*.MSFT")]
        [InlineData("*.*")]
        public void ShouldIncludeMatchingRoutableSubscriptionWithPattern(string contentPattern)
        {
            // Arrange
            var clientId = new ClientId("Client.1");
            _subscriptionIndex.AddSubscriptions(new[]
            {
                Subscription.Of<RoutableMessages.PriceUpdated>(clientId, ContentPattern.Split(contentPattern)),
            });

            var routableMessage = new RoutableMessages.PriceUpdated { ExchangeCode = "NASDAQ", Symbol = "MSFT" };

            // Act
            var clientIds = _router.GetConsumers(routableMessage).ToList();

            // Assert
            Assert.Equal(new[] { clientId }, clientIds);
        }

        [Theory]
        [InlineData("NASDAQ")]
        [InlineData("NASDAQ.42")]
        [InlineData("NASDAQ.*.*.*.MSFT")]
        [InlineData("NASDAQ.42.TECH.L.MSFT")]
        [InlineData("*")]
        [InlineData("*.*.*.*.MSFT")]
        [InlineData("*.*")]
        [InlineData("*.42.*.*.*")]
        [InlineData("*.*.*.*.*")]
        public void ShouldIncludeMatchingRoutableSubscriptionWithLongPattern(string contentPattern)
        {
            // Arrange
            var clientId = new ClientId("Client.1");
            var subscription = Subscription.Of<RoutableMessages.InstrumentConnected>(clientId, ContentPattern.Split(contentPattern));

            var otherClientId = new ClientId("Client.2");
            var otherSubscription = Subscription.Of<RoutableMessages.InstrumentConnected>(otherClientId, ContentPattern.Split("NYSE.*.*.*.*"));

            _subscriptionIndex.AddSubscriptions(new[] { subscription, otherSubscription });

            var routableMessage = new RoutableMessages.InstrumentConnected { ExchangeCode = "NASDAQ", ProviderId = 42, Sector = "TECH", SymbolRangeStart = 'L', Symbol = "MSFT" };

            // Act
            var clientIds = _router.GetConsumers(routableMessage).ToList();

            // Assert
            Assert.Equal(new[] { clientId }, clientIds);
        }

        [Theory]
        [InlineData("NYSE")]
        [InlineData("NASDAQ.AMZN")]
        [InlineData("NYSE.MSFT")]
        [InlineData("*.AMZN")]
        [InlineData("NYSE.*")]
        [InlineData("*.NASDAQ")]
        [InlineData("MSFT.NASDAQ")]
        public void ShouldExcludeSingleRoutableSubscriptionWithPattern(string contentPattern)
        {
            // Arrange
            var clientId = new ClientId("Client.1");
            _subscriptionIndex.AddSubscriptions(new[]
            {
                Subscription.Of<RoutableMessages.PriceUpdated>(clientId, ContentPattern.Split(contentPattern)),
            });

            var routableMessage = new RoutableMessages.PriceUpdated { ExchangeCode = "NASDAQ", Symbol = "MSFT" };

            // Act
            var clientIds = _router.GetConsumers(routableMessage).ToList();

            // Assert
            Assert.Empty(clientIds);
        }

        [Fact]
        public void ShouldSupportContentWithEmptyValue()
        {
            // Arrange
            var clientId = new ClientId("Client.1");
            _subscriptionIndex.AddSubscriptions(new[]
            {
                Subscription.Of<RoutableMessages.TradingHalted>(clientId, ContentPattern.Split("NASDAQ.*.*")),
            });

            var routableMessage = new RoutableMessages.TradingHalted { ExchangeCode = "NASDAQ", Symbol = "MSFT" };

            // Act
            var clientIds = _router.GetConsumers(routableMessage).ToList();

            // Assert
            Assert.Equal(new[] { clientId }, clientIds);
        }

        [Fact]
        public void ShouldRemoveSpecificSubscription()
        {
            // Arrange
            var clientId = new ClientId("Client.1");
            var subscription = Subscription.Of<RoutableMessages.PriceUpdated>(clientId, new ContentPattern("NASDAQ"));

            _subscriptionIndex.AddSubscriptions(new[] { subscription });

            // Ensure the client is subscribed
            var clientIds = _router.GetConsumers(new RoutableMessages.PriceUpdated { ExchangeCode = "NASDAQ", Symbol = "MSFT" }).ToList();
            Assert.Contains(clientId, clientIds);

            // Act
            _subscriptionIndex.RemoveSubscriptions(new[] { subscription });

            // Assert
            clientIds = _router.GetConsumers(new RoutableMessages.PriceUpdated { ExchangeCode = "NASDAQ", Symbol = "MSFT" }).ToList();
            Assert.DoesNotContain(clientId, clientIds);
        }

        [Fact]
        public void ShouldRemoveAllSubscriptionsForConsumer()
        {
            // Arrange
            var clientId = new ClientId("Client.1");
            _subscriptionIndex.AddSubscriptions(new[]
            {
                Subscription.Of<RoutableMessages.PriceUpdated>(clientId, new ContentPattern("NASDAQ")),
                Subscription.Of<RoutableMessages.InstrumentAdded>(clientId, new ContentPattern("4"))
            });
            // Ensure the client is subscribed
            var clientIdsSubscriptionA = _router.GetConsumers(new RoutableMessages.PriceUpdated { ExchangeCode = "NASDAQ", Symbol = "MSFT" }).ToList();
            var clientIdsSubscriptionB = _router.GetConsumers(new RoutableMessages.InstrumentAdded { ExchangeId = 4 }).ToList();

            Assert.Contains(clientId, clientIdsSubscriptionA);
            Assert.Contains(clientId, clientIdsSubscriptionB);

            // Act
            _subscriptionIndex.RemoveSubscriptionsForConsumer(clientId);

            // Assert
            var clientIds = _router.GetConsumers(new RoutableMessages.PriceUpdated { ExchangeCode = "NASDAQ", Symbol = "MSFT" }).ToList();
            Assert.DoesNotContain(clientId, clientIds);

            clientIds = _router.GetConsumers(new RoutableMessages.InstrumentAdded { ExchangeId = 4 }).ToList();
            Assert.DoesNotContain(clientId, clientIds);
        }

        [Fact]
        public void ShouldIncludeSubscriptionWithEmptyPattern()
        {
            // Arrange
            var clientId = new ClientId("Client.1");
            _subscriptionIndex.AddSubscriptions(new[]
            {
                Subscription.Of<RoutableMessages.PriceUpdated>(clientId, new ContentPattern()) // Empty pattern
            });

            var routableMessage = new RoutableMessages.PriceUpdated { ExchangeCode = "NASDAQ", Symbol = "MSFT" };

            // Act
            var clientIds = _router.GetConsumers(routableMessage).ToList();

            // Assert
            Assert.Equal(new[] { clientId }, clientIds); // Now we expect the client to receive the message
        }

        [Fact]
        public void ShouldNotDuplicateSubscription()
        {
            // Arrange
            var clientId = new ClientId("Client.1");
            var subscription = Subscription.Of<RoutableMessages.PriceUpdated>(clientId, new ContentPattern("NASDAQ"));

            _subscriptionIndex.AddSubscriptions(new[] { subscription });
            _subscriptionIndex.AddSubscriptions(new[] { subscription }); // Add the same subscription again

            // Act
            var clientIds = _router.GetConsumers(new RoutableMessages.PriceUpdated { ExchangeCode = "NASDAQ", Symbol = "MSFT" }).ToList();

            // Assert
            Assert.Equal(new[] { clientId }, clientIds); // Ensure no duplication
        }

    }
}
