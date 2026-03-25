using Elite.Auth.Token.Lib.Common;
using Elite.Common.Utilities.CommonType;
using Elite.Common.Utilities.Encription;
using Elite.OIDC.Handler.Lib.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.OIDC.Handler.Lib.Auth
{
    public class AuthCookiesHeader
    {
        private static CookieOptions CookieOptions(IOptions<AuthConfig> _settings) =>
            new CookieOptions()
            {
                Secure = true,
                HttpOnly = true,
                Expires = DateTimeOffset.Now.AddMinutes(_settings.Value.CookieTime)

            };


        public static void AddCookies(IOptions<AuthConfig> _settings, HttpResponse response, AuthReponse result, bool isprotected = false)
        {
            var options = AuthCookiesHeader.CookieOptions(_settings);

            AuthCookiesHeader.CreateCookie(_settings, response, JsonConvert.SerializeObject(result.Id.ToString()), "PInfo", options, true);
            AuthCookiesHeader.CreateCookie(_settings, response, result, "uid", options, true);
        }

        private static void CreateCookie(IOptions<AuthConfig> _settings, HttpResponse response, string result, string cookiename, CookieOptions options, bool isprotected = false)
        {

            response.Cookies.Append("CookieTime", _settings.Value.CookieTime.ToString());
            if (isprotected)
            {
                var encryptedString = AesCryption.EncryptString(result, _settings.Value.EncryptionKey);
                response.Cookies.Append(cookiename, encryptedString, options);
                //response.Headers.Add(cookiename, encryptedString);
            }
            else
            {
                response.Cookies.Append(cookiename, result, options);
            }
        }

        private static void CreateCookie(IOptions<AuthConfig> _settings, HttpResponse response, AuthReponse result, string cookiename, CookieOptions options, bool isprotected = false)
        {
            response.Cookies.Append("CookieTime", _settings.Value.CookieTime.ToString());
            if (isprotected)
            {
                var encryptedString = AesCryption.EncryptString(result.Uid, result.Id.ToString());
                response.Cookies.Append(cookiename, encryptedString, options);
                //response.Headers.Add(cookiename, encryptedString);
            }
            else
            {
                response.Cookies.Append(cookiename, result.Uid, options);
            }
        }


        public static string AuthorizationHeader(IOptions<AuthConfig> _settings)
        {
            try
            {
                string clientId = _settings.Value.Audience;
                string clientSecret = _settings.Value.ClientSecret;
                return Base64Encode(clientId + ":" + clientSecret);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        public static string AuthorizationHeaderForIntrospection(IOptions<AuthConfig> _settings)
        {
            try
            {
                string clientId = _settings.Value.IntrospectionId;
                string clientSecret = _settings.Value.IntrospectionSecret;
                return Base64Encode(clientId + ":" + clientSecret);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private static string Base64Encode(string plainText)
        {
            try
            {
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                return System.Convert.ToBase64String(plainTextBytes);
            }
            catch (Exception ex)
            {
                throw;
            }

        }
    }
}
