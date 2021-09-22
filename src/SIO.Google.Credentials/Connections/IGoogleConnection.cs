using Google.Apis.Auth.OAuth2;
using SIO.Infrastructure.Connections.Pooling;

namespace SIO.Google.Credentials.Connections
{
    public interface IGoogleConnection: IConnection
    {
        GoogleCredential Credential { get; }
    }
}
