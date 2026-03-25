using Elite.Common.Utilities.Encription;
using Elite.Common.Utilities.ExceptionHandling;
using Elite.Common.Utilities.HttpClientHelper;
using Elite.Common.Utilities.RequestContext;
using Elite.Task.Microservice.Application.CQRS.IntegrationEvents.Events;
using Elite_Task.Microservice.Application.CQRS.ExternalService;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Elite_Task.Microservice.Application.CQRS.ExternalService
{
    public class TopicServices : ITopicServices
    {

        protected readonly IConfiguration _configuration;
        private HttpClientHelper _httpHelper;
        private IRequestContext _context;

        public TopicServices(IConfiguration configuration,IRequestContext context)
        {
            this._configuration = configuration;
            this._context = context;
            //this._httpHelper = new HttpClientHelper(_configuration.GetSection("TopicService:BaseUrl").Value,context.tokenInfo, context.UID);
        }
        public async System.Threading.Tasks.Task PublishTopicHistoryAsync(TopicHistoryEvent evt)
        {
            await  Post(evt);
        }

        private async Task Post(TopicHistoryEvent evt)
        {
            this._httpHelper = new HttpClientHelper(_configuration.GetSection("TopicService:BaseUrl").Value, false);
            this._httpHelper.SetRequestHeaderForSecureID(_context);
            HttpResponseMessage topicPostResponse = null;
            var contentTopic = new StringContent(JsonConvert.SerializeObject(evt), UTF8Encoding.UTF8, "application/json");

            topicPostResponse = await _httpHelper.HttpClient.PostAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("TopicService:ApiLink:TopicHistory").Value, contentTopic);

            if ((int)topicPostResponse.StatusCode != (int)System.Net.HttpStatusCode.OK)
                throw new EliteException($" Api call was failed { string.Join('/', _configuration.GetSection("TopicService:BaseUrl").Value, _configuration.GetSection("TopicService:ApiLink:TopicHistory").Value)}  with status code - {((int)topicPostResponse.StatusCode)} ");
        }
    }
}
