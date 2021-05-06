using System;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;

namespace SIO.Google.Credentials.Connections
{
    internal sealed class GoogleConnection : IGoogleConnection
    {
        private readonly GoogleCredential _underlyingConnection;

        public string ConnectionId { get; }

        public GoogleConnection(GoogleCredentialOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _underlyingConnection = GoogleCredential.FromJson(JsonConvert.SerializeObject(options));
        }

        public GoogleCredential Credential => _underlyingConnection;
    }
}
