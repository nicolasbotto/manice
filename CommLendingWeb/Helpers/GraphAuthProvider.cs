using CommLendingWeb.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CommLendingWeb.Helpers
{
	public class GraphAuthProvider : IGraphAuthProvider
	{
		private readonly IMemoryCache memoryCache;
		private TokenCache userTokenCache;

		// Properties used to get and manage an access token.
		private readonly string appId;
		private readonly ClientCredential credential;
		private readonly string[] scopes;
		private readonly string redirectUri;
		private readonly IHttpContextAccessor contextAccessor;
		private readonly string secret;

		public GraphAuthProvider(IMemoryCache memoryCache, IConfiguration configuration, IHttpContextAccessor contextAccessor)
		{
			var azureOptions = new AzureAdOptions();
			configuration.Bind("AzureAd", azureOptions);

			this.appId = azureOptions.ClientId;
			this.credential = new ClientCredential(azureOptions.ClientSecret);
			this.scopes = azureOptions.GraphScopes.Split(new[] { ' ' });
			this.redirectUri = azureOptions.BaseUrl + azureOptions.CallbackPath;
			this.secret = azureOptions.ClientSecret;
			this.memoryCache = memoryCache;
			this.contextAccessor = contextAccessor;
		}

		public async Task<string> GetTokenOnBehalfOfAsync()
		{
			try
			{
				const string siteUrl = "localhost:44334";// "proposalcreation.azurewebsites.net"; //localhost:44334
				// Get the raw token that the add-in page received from the Office host.
				var bootstrapContext = ((ClaimsIdentity)contextAccessor.HttpContext.User.Identity).BootstrapContext.ToString();

				UserAssertion userAssertion = new UserAssertion(bootstrapContext);

				// Get the access token for MS Graph. 
				ClientCredential clientCred = new ClientCredential(this.secret);
				ConfidentialClientApplication cca =
					new ConfidentialClientApplication(appId,
														$"https://{siteUrl}", clientCred, null, null);

				// The AcquireTokenOnBehalfOfAsync method will first look in the MSAL in memory cache for a
				// matching access token. Only if there isn't one, does it initiate the "on behalf of" flow
				// with the Azure AD V2 endpoint.
				AuthenticationResult result = await cca.AcquireTokenOnBehalfOfAsync(scopes, userAssertion, "https://login.microsoftonline.com/common/oauth2/v2.0");
				return result.AccessToken;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}


		// Gets an access token. First tries to get the access token from the token cache.
		// Using password (secret) to authenticate. Production apps should use a certificate.
		public async Task<string> GetUserAccessTokenAsync(string userId)
		{
			userTokenCache = new SessionTokenCache(userId, memoryCache).GetCacheInstance();

			var cca = new ConfidentialClientApplication(
				appId,
				redirectUri,
				credential,
				userTokenCache,
				null);

			if (!cca.Users.Any()) throw new ServiceException(new Error
			{
				Code = "TokenNotFound",
				Message = "User not found in token cache. Maybe the server was restarted."
			});

			try
			{
				var result = await cca.AcquireTokenSilentAsync(scopes, cca.Users.First());
				return result.AccessToken;
			}

			// Unable to retrieve the access token silently.
			catch (Exception)
			{
				throw new ServiceException(new Error
				{
					Code = GraphErrorCode.AuthenticationFailure.ToString(),
					Message = "Caller needs to authenticate. Unable to retrieve the access token silently."
				});
			}
		}
	}

	public interface IGraphAuthProvider
	{
		Task<string> GetUserAccessTokenAsync(string userId);
		Task<string> GetTokenOnBehalfOfAsync();
	}
}