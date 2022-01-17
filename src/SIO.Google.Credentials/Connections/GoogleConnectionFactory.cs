using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using SIO.Infrastructure.Connections.Pooling;

namespace SIO.Google.Credentials.Connections
{
    internal sealed class GoogleConnectionFactory : IConnectionFactory<GoogleConnection>
    {
        private readonly IEnumerable<GoogleConnection> _googleConnections;

        public GoogleConnectionFactory(IOptions<GoogleConnectionOptions> options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _googleConnections = options.Value.AvaliableCredentials.Select(o => new GoogleConnection(o));
        }

        public IEnumerable<GoogleConnection> CreateConnections()
        {
            return _googleConnections;
        }
    }
}
