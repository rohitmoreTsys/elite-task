using Elite.Auth.Token.Lib.Command;
using Elite.Auth.Token.Lib.Common;
using Elite.Auth.Token.Lib.Services;
using Elite.Common.Utilities.CommonType;
using Elite.OIDC.Handler.Lib.Model;
using IdentityModel;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Elite.OIDC.Handler.Lib.Auth
{
    public class AuthMethodIntrospection
    {
        public static async Task<AuthReponse> TokenIntrospectionAndRefreshToken(IAuthTokenServices authTokenServices, IOptions<AuthConfig> _settings, AuthTokenCommand authTokenCommand)
        {
            try
            {
                var authReponse = new AuthReponse();
                using (var client = new HttpClient())
                {
                    string authHeader = AuthCookiesHeader.AuthorizationHeaderForIntrospection(_settings);
                    string apiUrl = _settings.Value.Introspection;

                    client.DefaultRequestHeaders.Add("Authorization", "Basic " + authHeader);

                    var formContent = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("token",authTokenCommand.AccessToken),
                        new KeyValuePair<string, string>("token_type_hint", "access_token")
                    });

                    var response = await client.PostAsync(apiUrl, formContent);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var stringContent = response.Content.ReadAsStringAsync().Result;
                        dynamic TokenResponse = JsonConvert.DeserializeObject(stringContent);
                        if (Convert.ToBoolean(TokenResponse?.active))
                        {
                            authReponse.Id = authTokenCommand.Id;
                            authReponse.Uid = authTokenCommand.Uid;
                            return authReponse;
                        }
                        else
                            return await GetRefreshedToken(authTokenServices, authTokenCommand.RefreshToken, _settings, authTokenCommand.Id);
                    }
                    else
                        return await GetRefreshedToken(authTokenServices, authTokenCommand.RefreshToken, _settings, authTokenCommand.Id);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<HttpResponseMessage> GetRefresh(string refresh_token, IOptions<AuthConfig> _settings)
        {
            using (var client = new HttpClient())
            {
                // string apiUrl = "https://sso-int.e.corpintra.net/as/token.oauth2";
                string apiUrl = _settings.Value.TokenUrl;

                string authHeader = AuthCookiesHeader.AuthorizationHeader(_settings);


                var formContent = new FormUrlEncodedContent(new[]
                {
                     new KeyValuePair<string, string>("grant_type", "refresh_token"),
                     new KeyValuePair<string, string>( "refresh_token", refresh_token)//,
                     // new KeyValuePair<string, string>( "redirect_uri", "https://elite-int.rd.corpintra.net/*/")
                 });

                client.DefaultRequestHeaders.Add("Authorization", "Basic " + authHeader);

                //var req = new HttpRequestMessage(HttpMethod.Post, apiUrl + "token.oauth2") { Content = new FormUrlEncodedContent(postData) };
                var response = await client.PostAsync(apiUrl, formContent);
                return response;

            }
        }

        public static string ValidateJwt(OIDCModel oidcModel, IOptions<AuthConfig> _settings)
        {
            var keys = new List<SecurityKey>();

            using (var client = new HttpClient())
            {
                string apiUrl = _settings.Value.JWKSVal;// "https://sso-int.e.corpintra.net/pf/JWKS";// "https://sso-int.e.corpintra.net/as/token.oauth2";


                var response = client.GetAsync(apiUrl).Result;
                var stringContent = response.Content.ReadAsStringAsync().Result;
                dynamic accessTokenResponse = JsonConvert.DeserializeObject(stringContent);
                foreach (var webKey in accessTokenResponse.keys)
                {

                    var exponent = webKey["e"] == null ? null : Base64Url.Decode(Convert.ToString(webKey.e));

                    var modulus = webKey["n"] == null ? null : Base64Url.Decode(Convert.ToString(webKey.n));

                    if (exponent != null)
                    {
                        var key = new RsaSecurityKey(new RSAParameters { Exponent = exponent, Modulus = modulus })
                        {
                            KeyId = webKey.Kid
                        };

                        keys.Add(key);
                    }
                }

            }


            var parameters = new TokenValidationParameters
            {
                ValidIssuer = _settings.Value.Issuer,
                ValidAudience = _settings.Value.Audience,
                IssuerSigningKeys = keys,

                NameClaimType = JwtClaimTypes.Subject,
                RoleClaimType = JwtClaimTypes.Role,


                RequireSignedTokens = true
            };
            try
            {
                var handler = new JwtSecurityTokenHandler();
                SecurityToken securityToken;
                var user = handler.ValidateToken(oidcModel.id_token, parameters, out securityToken);
                handler.InboundClaimTypeMap.Clear();
                var jwtSecurityToken = securityToken as JwtSecurityToken;
                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.RsaSha256, StringComparison.InvariantCultureIgnoreCase))
                    throw new SecurityTokenException("Invalid token");
                return user.Identity.Name;
            }
            catch (Exception ex)
            {
                if (typeof(SecurityTokenExpiredException) == ex.GetType())
                {
                    return "";//GetRefresh(oidcModel.refresh_token);
                }

            }
            return "";

        }

        private static async Task<AuthReponse> GetRefreshedToken(IAuthTokenServices authTokenServices, string refreshToken, IOptions<AuthConfig> _settings, Guid id)
        {
            var response = await GetRefresh(refreshToken, _settings);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var stringContent = response.Content.ReadAsStringAsync().Result;
                dynamic TokenResponse = JsonConvert.DeserializeObject(stringContent);

                string AccessCode = TokenResponse?.access_token;

                var token = new AuthTokenCommand
                {
                    Uid = AuthMethod.GetUserId(_settings, AccessCode),
                    AccessToken = AccessCode,
                    RefreshToken = TokenResponse?.refresh_token,
                    TokenExpireDateTime = AuthTokenHalper.GetTokenExpireDateTime((int)TokenResponse?.expires_in),
                    Id = id,
                    SourceId = _settings.Value.ServiceId
                };

                //Token -->  DB
                return await authTokenServices.PostAuthToken(token);
            }

            return null;
        }
    }


}
