using Elite.Auth.Token.Lib.Repository;
using Elite.Common.Utilities.CommonType;
using Elite.OIDC.Handler.Lib.Auth;
using Elite.OIDC.Handler.Lib.Model;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;

namespace Elite.OIDC.Handler.Lib.Auth
{
    public class AuthMethod
    {
        public static OIDCModel GetAccessCode(IOptions<AuthConfig> _settings, string authCode)
        {
            string AccessCode = string.Empty;
            string userId = string.Empty;
            OIDCModel oidcModel = new OIDCModel();

            try
            {
                using (var client = new HttpClient())
                {
                    string apiUrl = _settings.Value.TokenUrl;
                    string authHeader = AuthCookiesHeader.AuthorizationHeader(_settings);


                    var formContent = new FormUrlEncodedContent(new[]
                    {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>( "code", authCode),
             new KeyValuePair<string, string>( "redirect_uri", _settings.Value.RedirectUrl)
            });

                    client.DefaultRequestHeaders.Add("Authorization", "Basic " + authHeader);
                   // client.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");

                    //var req = new HttpRequestMessage(HttpMethod.Post, apiUrl + "token.oauth2") { Content = new FormUrlEncodedContent(postData) };
                    var response = client.PostAsync(apiUrl, formContent).Result;
                    var stringContent = response.Content.ReadAsStringAsync().Result;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {

                        dynamic accessTokenResponse = JsonConvert.DeserializeObject(stringContent);

                        AccessCode = accessTokenResponse?.access_token;
                        userId = GetUserId(_settings, AccessCode);
                        oidcModel.access_token = AccessCode;
                        oidcModel.refresh_token = accessTokenResponse?.refresh_token;
                        oidcModel.TokenExpires = accessTokenResponse?.expires_in;
                        oidcModel.userId = userId;
                    }
                    else
                    {
                        throw new Exception($"{stringContent} , Status code - {(int)response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return oidcModel;
        }
        
        public static string GetUserId(IOptions<AuthConfig> _settings, string accessToken)
        {
            string userId = string.Empty;
            try
            {
                using (var client = new HttpClient())
                {
                    string apiUrl = _settings.Value.UserInfoUrl;
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
                    var response = client.GetAsync(apiUrl).Result;
                    var stringContent = response.Content.ReadAsStringAsync().Result;
                    dynamic profileResponse = JsonConvert.DeserializeObject(stringContent);
                    userId = profileResponse?.sub;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return userId;
        }

    }
}
