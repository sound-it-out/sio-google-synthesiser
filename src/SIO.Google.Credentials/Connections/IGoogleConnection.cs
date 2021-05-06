using System;
using Google.Apis.Auth.OAuth2;

namespace SIO.Google.Credentials.Connections
{
    public interface IGoogleConnection
    {
        GoogleCredential Credential { get; }
    }
}
