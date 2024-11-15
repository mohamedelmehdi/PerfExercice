// Copyright (C) Abc Arbitrage Asset Management - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Written by Olivier Coanet <o.coanet@abc-arbitrage.com>, 2020-10-01

namespace AbcArbitrage.Homework.Routing
{
    public interface IRoutableMessage : IMessage
    {
        MessageRoutingContent GetContent();
    }
}
