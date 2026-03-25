using Elite.Auth.Token.Lib.Command;
using Elite.Auth.Token.Lib.Common;
using Elite.Auth.Token.Lib.Models;
using Elite.Auth.Token.Lib.Models.Entities;
using Elite.Common.Utilities.ExceptionHandling;
using Elite.Common.Utilities.HttpClientHelper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Elite.Auth.Token.Lib.Services
{
    public class AuthTokenServices : IAuthTokenServices
    {
        protected readonly IConfiguration _configuration;
        private readonly EliteAuthTokenContext _eliteAuthContext;

        //private HttpClientHelper _httpHelper;

        public AuthTokenServices(IConfiguration configuration, EliteAuthTokenContext eliteAuthTokenContext)
        {
            this._configuration = configuration;
            _eliteAuthContext = eliteAuthTokenContext;
        }
        public AuthTokenServices(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public async Task<AuthTokenCommand> GetToken(Guid guid)
        {
           var _httpHelper = new HttpClientHelper(_configuration.GetSection("EliteAuthApi:BaseUrl").Value);
            var topicGetResponse = await _httpHelper.HttpClient.GetAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("EliteAuthApi:ApiLink:GetToken").Value + guid);
            if ((int)topicGetResponse.StatusCode == (int)System.Net.HttpStatusCode.OK)
            {
                var committee = JsonConvert.DeserializeObject<AuthTokenCommand>(await topicGetResponse.Content.ReadAsStringAsync());
                if (committee != null)
                    return committee;
            }
            return null;
        }


        public async Task<bool> IsRequestProcessing(Guid guid)
        {
            var _httpHelper = new HttpClientHelper(_configuration.GetSection("EliteAuthApi:BaseUrl").Value);
            var topicGetResponse = await _httpHelper.HttpClient.GetAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("EliteAuthApi:ApiLink:IsRequestProcessing").Value + guid);
            if ((int)topicGetResponse.StatusCode == (int)System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<bool>(await topicGetResponse.Content.ReadAsStringAsync());
            }
            return false;
        }

        public async Task<AuthReponse> PostAuthToken(AuthTokenCommand token)
        {

            var _httpHelper = new HttpClientHelper(_configuration.GetSection("EliteAuthApi:BaseUrl").Value);
            var content = new StringContent(JsonConvert.SerializeObject(token), UTF8Encoding.UTF8, "application/json");
            var response = await _httpHelper.HttpClient.PostAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("EliteAuthApi:ApiLink:PostAuthToken").Value, content);
            if ((int)response.StatusCode != (int)System.Net.HttpStatusCode.OK)
                throw new EliteException($" Post Auth token failed with http status code - { (int)(response.StatusCode)} in { nameof(AuthTokenServices)}");
            else
                return JsonConvert.DeserializeObject<AuthReponse>(await response.Content.ReadAsStringAsync());
        }

        public async Task<bool> GetReviewStatus(string bundleId, string version)
        {
            var _httpHelper = new HttpClientHelper(_configuration.GetSection("EliteAuthApi:BaseUrl").Value);
            var reviewStatus = await _httpHelper.HttpClient.GetAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("EliteAuthApi:ApiLink:GetToken").Value+ "GetReviewStatus/" + bundleId + "/" + version);
            return JsonConvert.DeserializeObject<bool>(await reviewStatus.Content.ReadAsStringAsync());
        }
        }
}
