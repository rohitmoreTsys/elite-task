using Elite.Common.Utilities.Encription;
using Elite.Common.Utilities.RequestContext;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Elite.Common.Utilities.HttpClientHelper
{
    public class HttpClientHelper
    {

        private HttpClient _httpClient;
        public HttpClientHelper(string baseUrl, bool allowInterserviceCall = false)
        {

            //HttpClientHandler handler = new HttpClientHandler();
            //handler.CookieContainer = new CookieContainer();

            //handler.CookieContainer.Add(new Uri(baseUrl), new Cookie("PInfo", cookie)); // Adding a Cookie
            //_httpClient = new HttpClient(handler);



            HttpClientHandler handler = new HttpClientHandler
            {
                UseCookies = true,
                UseDefaultCredentials = true,
                CookieContainer = new CookieContainer()//.Add(new Cookie("PInfo", cookie),)
            };

            //handler.CookieContainer.Add(new Uri(baseUrl), createCookie("PInfo", cookie)); // Adding a Cookie
            //handler.CookieContainer.Add(new Uri(baseUrl), createCookie("uid", uid));
            handler.CookieContainer.Add(new Uri(baseUrl), createCookie("AllowInterserviceCall", allowInterserviceCall.ToString()));

            _httpClient = new HttpClient(handler);

            _httpClient.BaseAddress = new Uri(baseUrl);


            //_httpClient = new HttpClient();
            //HttpClient.BaseAddress = new Uri(baseUrl);

        }

        public HttpClientHelper(string baseUrl)
        {
            _httpClient = new HttpClient();
            HttpClient.BaseAddress = new Uri(baseUrl);

        }

        public HttpClient HttpClient { get { return _httpClient; } }

        public void SetRequestHeader(IConfiguration configuration, string uid, string deputy)
        {
            HttpClient.DefaultRequestHeaders.Add(configuration.GetSection("RequestHeaders:UIDRequestHeaderKey").Value, uid);
            HttpClient.DefaultRequestHeaders.Add(configuration.GetSection("RequestHeaders:DeputyUIDRequestHeaderKey").Value, deputy);
        }
		public void SetRescheduleRequestHeader(IConfiguration configuration, List<long> topicIds)
		{
			HttpClient.DefaultRequestHeaders.Add(configuration.GetSection("RequestHeaders:TopicIdsRequestHeaderKey").Value, JsonConvert.SerializeObject(topicIds));
		}
		public void SetRequestHeaderForMailsIds(IConfiguration configuration, string uid, string deputy)
        {
            HttpClient.DefaultRequestHeaders.Add(configuration.GetSection("RequestHeaders:RequestedByUidHeaderKey").Value, uid);
            HttpClient.DefaultRequestHeaders.Add(configuration.GetSection("RequestHeaders:CreatedByUidHeaderKey").Value, deputy);
        }
        public void SetRequestHeaderForSecureID(IRequestContext context)
        {
            if(context.IsHttpContextExist && !string.IsNullOrEmpty(context.SecuredUID))
                HttpClient.DefaultRequestHeaders.Add("securedUID", context.SecuredUID);
        }
        public void SetSecureUIDHeader_SIGMAEQ(String securedUID)
        {
                HttpClient.DefaultRequestHeaders.Add("securedUID", securedUID);
        }

        private Cookie createCookie(string cookieName, string cookieValue)
        {
            Cookie cookie = new Cookie(cookieName, cookieValue);
            cookie.Expires = DateTime.Now.AddMinutes(600);
            return cookie;
        }
    }
}
