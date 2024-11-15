// Copyright (C) Abc Arbitrage Asset Management - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Written by Olivier Coanet <o.coanet@abc-arbitrage.com>, 2020-10-01

using System;

namespace AbcArbitrage.Homework.Routing
{
    public readonly struct ClientId : IEquatable<ClientId>
    {
        private readonly string _value;

        public ClientId(string value) => _value = value;

        public bool Equals(ClientId other) => _value == other._value;

        public override bool Equals(object? obj) => obj is ClientId other && Equals(other);

        public override int GetHashCode() => _value.GetHashCode();

        public override string ToString() => _value;
    }
}
