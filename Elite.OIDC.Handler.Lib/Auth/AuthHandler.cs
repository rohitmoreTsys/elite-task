
using Elite.Auth.Token.Lib.Common;
using Elite.Auth.Token.Lib.Services;
using Elite.Common.Utilities.CommonType;
using Elite.Common.Utilities.Encription;
using Elite.OIDC.Handler.Lib.Auth;
using Elite.OIDC.Handler.Lib.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Elite.OIDC.Handler.Lib
{
    public class AuthHandler : AuthenticationHandler<AuthSchemeOptions>
    {
        private const string UnAuthorize_PInfo = "Unauthorized User - No PInfo Header";
        private const string UnAuthorize_UidMiss = "Unauthorized User - Login User Missing";
        private const string UnAuthorize_InvalidUid = "Unauthorized User - Invalid uid";
        private const string UnAuthorize_InvalidToken = "Unauthorized User - Invalid Token";

		// private IDataProtectorBase _provider;
		ILoggerFactory _logger;
		ILogger<AuthHandler> logging;
		private IOptions<AuthConfig> _settings;
		private readonly IAuthTokenServices _authTokenServices;
		private readonly IConfiguration _configuration;
		private static string key;




		public AuthHandler(IOptionsMonitor<AuthSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock,
			IOptions<AuthConfig> settings, Func<IConfiguration, IAuthTokenServices> tokenServiceFactory, IConfiguration configuration)//, IDataProtectorBase provider)
			: base(options, logger, encoder, clock)
		{
			_options = options;

			logging = logger.CreateLogger<AuthHandler>();
			_settings = settings;
			_configuration = configuration;
			_authTokenServices = tokenServiceFactory(configuration);
		}

		public IOptionsMonitor<AuthSchemeOptions> _options { get; private set; }

		protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			try
			{

				#region Commented
				/// test code
				// get Access Token 
				// var tokenDttest = await _authTokenServices.GetToken(Guid.Parse("114aff9d-25a7-4440-9796-01b61f2b8045"));
				//Post token

                //var authTokenId = await _authTokenServices.PostAuthToken(new AuthTokenCommand
                //{
                //    AccessToken ="string",
                //    CreateDate = DateTime.Now,
                //    IDToken = "string",
                //    RefreshToken ="string",
                //    Uid ="lsasaslslssls",
                //    TokenExpireDateTime = DateTime.Now
                //});
                /// end test code  3
                #endregion


                key = _settings.Value.EncryptionKey;

                if (Request.Cookies.TryGetValue("AllowInterserviceCall", out var AllowInterserviceCall))
                {
                    if (!Convert.ToBoolean(AllowInterserviceCall))
                        return await Task.FromResult(AuthenticateResult.Success(GetAuthTickets("Anynomous")));
                }

                if (!Request.Cookies.TryGetValue("PInfo", out var PInfo))
                {
                   return await Task.FromResult(AuthenticateResult.Fail(UnAuthorize_PInfo));
                }

				if (!GetGuid(PInfo, out string id))
				{
					return await Task.FromResult(AuthenticateResult.Fail(UnAuthorize_PInfo));
				}


                if (!Request.Cookies.TryGetValue("uid", out var uid))
                {
                    return await Task.FromResult(AuthenticateResult.Fail(UnAuthorize_PInfo));
                }

				Guid guid = Guid.Parse(JsonConvert.DeserializeObject<string>(id.ToString())); //Auth Guid
				uid = AesCryption.DecryptString(uid.ToString(), guid.ToString());

                //To check user id in header and param are the same.
                string uidFromHeader = GetHttpRequestHeader(Request, "uid");
				//check if multiple UIDs are present
				if(!string.IsNullOrEmpty(uidFromHeader))
                {
					logging.LogCritical($"UID from header -->  UID : {uidFromHeader}");
					if (uidFromHeader.Contains(','))
                    {
						string[] uidArray = uidFromHeader.Split(',');
                        if (uidArray[0] == uidArray[1])
                        {
							uidFromHeader = uidArray[0];
                        } 
						else
                        {
							logging.LogCritical($"Different UIDs are present in header -->  UID : {uidFromHeader}");
							return await Task.FromResult(AuthenticateResult.Fail(UnAuthorize_InvalidUid));
						}
					}
                }
				
                				
				if(string.IsNullOrEmpty(uidFromHeader) || string.IsNullOrEmpty(uid))
				{
                    return await Task.FromResult(AuthenticateResult.Fail(UnAuthorize_InvalidUid));
                }
				
				if (uidFromHeader.ToUpper() != uid.ToUpper())
                {
                    return await Task.FromResult(AuthenticateResult.Fail(UnAuthorize_InvalidUid));
                }

                SetHttpRequestHeader(Request, "uid", uid);

			

				// Auth Api call to get the Token details
				var tokenDt = await _authTokenServices.GetToken(guid);

                if (tokenDt is null)
                    return await Task.FromResult(AuthenticateResult.Fail(UnAuthorize_UidMiss));

                //if (tokenDt.IsRefreshTokenGenerated.HasValue && tokenDt.IsRefreshTokenGenerated.Value)
                //    return await Task.FromResult(AuthenticateResult.Success(GetAuthTickets(uid)));

                // Validate uid
                if (tokenDt.Uid.ToUpper() != uid.ToString().ToUpper())
                    return await Task.FromResult(AuthenticateResult.Fail(UnAuthorize_InvalidUid));

				// Validate the Expiry of Tokens
				if (tokenDt.TokenExpireDateTime.ToUniversalTime() < DateTime.Now.ToUniversalTime())
				{
					if (await _authTokenServices.IsRequestProcessing(guid))
					{
						return await Task.FromResult(AuthenticateResult.Success(GetAuthTickets(uid)));
					}

					logging.LogCritical($"Request For Introspection, UID : {tokenDt.Uid} , Guid : {tokenDt.Id}");
					var authReponse = await AuthMethodIntrospection.TokenIntrospectionAndRefreshToken(_authTokenServices, _settings, tokenDt);
					if (authReponse is null)
					{
						return await Task.FromResult(AuthenticateResult.Fail(UnAuthorize_InvalidToken));
					}

					if (authReponse.IsTokenRefresh)
					{
						logging.LogCritical($"Access Token Expired__Request For Refresh Token -->  UID : {tokenDt.Uid} , Guid : {tokenDt.Id}");
					}
					else
					{
						logging.LogCritical($"Access Token Still Valid --> UID : {tokenDt.Uid} , Guid : {tokenDt.Id}");
					}

                    AuthCookiesHeader.AddCookies(_settings, Response, authReponse, true);

                    logging.LogCritical($"Completed Introspection -->  UID : {tokenDt.Uid} , Guid : {tokenDt.Id}");
                }
                else
                {
                    AuthCookiesHeader.AddCookies(_settings, Response, new AuthReponse { Id = guid, Uid = uid }, true);
                }

				return await Task.FromResult(AuthenticateResult.Success(GetAuthTickets(uid)));
			}
			catch (Exception ex)
			{
				return await Task.FromResult(AuthenticateResult.Fail(ex.Message));
			}
		}

		private string GetHttpRequestHeader(HttpRequest httpRequest, string key)
		{
			if (httpRequest.Headers.ContainsKey(key))
			{
				return httpRequest.Headers[key];
			}
			return string.Empty;
		}
		private bool GetGuid(string pInfo, out string id)
		{
			id = string.Empty;
			if (!string.IsNullOrEmpty(pInfo.ToString()))
			{
				id = AesCryption.DecryptString(pInfo, key);
				if (!string.IsNullOrEmpty(id))
					return true;
				else
					return false;
			}
			return false;
		}


		private void SetHttpRequestHeader(HttpRequest httpRequest, string key, string value)
		{
			if (httpRequest.Headers.ContainsKey(key))
			{
				httpRequest.Headers[key] = value;
			}
			else
			{
				httpRequest.Headers.Add(key, value);
			}
		}


		private AuthenticationTicket GetAuthTickets(string user)
		{
			var identities = new List<ClaimsIdentity> { new ClaimsIdentity(user) };
			return new AuthenticationTicket(new ClaimsPrincipal(identities), Options.Scheme);
		}

		internal class CacheTicketStore : ITicketStore
		{
			private object cache;

			public CacheTicketStore(object cache)
			{
				this.cache = cache;
			}

			public Task RemoveAsync(string key)
			{
				throw new NotImplementedException();
			}

			public Task RenewAsync(string key, AuthenticationTicket ticket)
			{
				throw new NotImplementedException();
			}

			public Task<AuthenticationTicket> RetrieveAsync(string key)
			{
				throw new NotImplementedException();
			}

			public Task<string> StoreAsync(AuthenticationTicket ticket)
			{
				throw new NotImplementedException();
			}
		}
	}
}