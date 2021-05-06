using System;
using SIO.Infrastructure;

namespace SIO.Google.Credentials.Connections
{
    public readonly struct ConnectionId : IEquatable<ConnectionId>
    {
        internal string Value { get; }

        internal ConnectionId(string value)
        {
            Value = value;
        }

        public static ConnectionId New()
        {
            var value = Base64UrlIdGenerator.New();

            return new ConnectionId(value);
        }
        public static ConnectionId From(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"'{nameof(value)}' cannot be null or empty.", nameof(value));

            return new ConnectionId(value);
        }

        public bool Equals(ConnectionId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is ConnectionId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value;

        public static bool operator ==(ConnectionId left, ConnectionId right) => left.Equals(right);
        public static bool operator !=(ConnectionId left, ConnectionId right) => !left.Equals(right);
        public static implicit operator string(ConnectionId id) => id.Value;
    }
}
