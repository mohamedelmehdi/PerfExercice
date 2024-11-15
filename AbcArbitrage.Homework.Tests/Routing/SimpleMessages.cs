// Copyright (C) Abc Arbitrage Asset Management - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Written by Olivier Coanet <o.coanet@abc-arbitrage.com>, 2020-10-06

using System;

namespace AbcArbitrage.Homework.Routing
{
    public static class SimpleMessages
    {
        public class ExchangeAdded : IMessage
        {
            public int ExchangeId { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class ExchangeTradingPhaseChanged : IMessage
        {
            public int ExchangeId { get; set; }
            public int TradingPhaseId { get; set; }
            public DateTime TimestampUtc { get; set; }
        }
    }
}
