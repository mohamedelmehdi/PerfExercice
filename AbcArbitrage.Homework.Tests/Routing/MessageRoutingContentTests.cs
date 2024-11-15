// Copyright (C) Abc Arbitrage Asset Management - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Written by Olivier Coanet <o.coanet@abc-arbitrage.com>, 2020-10-06

using System;
using Xunit;

namespace AbcArbitrage.Homework.Routing
{
    public class MessageRoutingContentTests
    {
        [Fact]
        public void ShouldGetContentWithEmptyValue()
        {
            // Arrange
            var message = new RoutableMessages.TradingHalted
            {
                ExchangeCode = "NASDAQ",
                Symbol = "AMZN",
                TimestampUtc = DateTime.UtcNow,
            };

            // Act
            var routingContent = MessageRoutingContent.FromMessage(message);

            // Assert
            Assert.Equal(new[] { "NASDAQ", string.Empty, "AMZN" }, routingContent.Parts);
        }
    }
}
