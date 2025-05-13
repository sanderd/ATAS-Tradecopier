using HttpTracer;
using HttpTracer.Logger;
using RestSharp;
using RestSharp.Authenticators;

namespace sadnerd.io.ATAS.ProjectXApiClient;

public class ProjectXAuthenticator : AuthenticatorBase
{
    readonly string _baseUrl;
    private readonly string _apiKey;
    private readonly string _username;

    public ProjectXAuthenticator(string baseUrl, string apiKey, string username) : base("")
    {
        _baseUrl = baseUrl;
        _apiKey = apiKey;
        _username = username;
    }

    protected override async ValueTask<Parameter> GetAuthenticationParameter(string accessToken)
    {
        Token = string.IsNullOrEmpty(Token) ? await GetToken() : Token;
        return new HeaderParameter(KnownHeaders.Authorization, "Bearer " + Token);
    }

    async Task<string> GetToken()
    {
        var options = new RestClientOptions(_baseUrl)
        {
            ConfigureMessageHandler = handler => new HttpTracerHandler(handler, new ConsoleLogger(), HttpMessageParts.All)
            //Authenticator = new HttpBasicAuthenticator(_clientId, _clientSecret),
        };
        using var client = new RestClient(options);

        var request = new RestRequest("api/Auth/loginKey")
            .AddJsonBody(new
                {
                    userName = _username,
                    apiKey = _apiKey
                }
            );
        request.AddHeader("Accept", "text/plain");

        var response = await client.PostAsync<TokenResponse>(request);

        return response?.Token;
    }
}