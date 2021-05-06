using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Options;
using SIO.Infrastructure.Events;

namespace SIO.Google.Credentials.Connections
{
    internal sealed class GoogleConnectionPool : IGoogleConnectionPool
    {
        private readonly BlockingQueue<GoogleConnection> _availableConnections;
        private readonly ConcurrentDictionary<ConnectionId, GoogleConnection> _scopedConnections;
        private readonly object _scopedConnectionsLock = new object();
        private readonly object _avaliableConnectionsLock = new object();

        public GoogleConnectionPool(IOptions<GoogleConnectionOptions> options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var credentials = options.Value;
            _availableConnections = new BlockingQueue<GoogleConnection>(credentials.AvaliableCredentials.Select(o => new GoogleConnection(o)).ToArray());
            _scopedConnections = new ConcurrentDictionary<ConnectionId, GoogleConnection>();
        }

        public IGoogleConnection GetConnection(StreamId streamId, CancellationToken cancellationToken = default)
        {
            lock (_scopedConnectionsLock)
            {
                if (_scopedConnections.TryGetValue(ConnectionId.From(streamId), out var connection))
                    return connection;
            }

            lock (_avaliableConnectionsLock)
            {
                if (_availableConnections.TryDequeue(out var connection, cancellationToken))
                {
                    lock (_scopedConnectionsLock)
                    {
                        if (_scopedConnections.TryAdd(ConnectionId.From(streamId), connection))
                        {
                            return connection;
                        }
                        else
                        {
                            _availableConnections.Enqueue(connection);
                            return GetConnection(streamId);
                        }
                    }
                }
            }

            throw new InvalidOperationException("Unable to get a connection");
        }

        public void ReleaseConnection(StreamId streamId)
        {
            lock (_scopedConnectionsLock)
            {
                if (_scopedConnections.TryRemove(ConnectionId.From(streamId), out var connection))
                {
                    lock(_avaliableConnectionsLock)
                    {
                        _availableConnections.Enqueue(connection);
                    }
                }
            }
        }
    }
}
