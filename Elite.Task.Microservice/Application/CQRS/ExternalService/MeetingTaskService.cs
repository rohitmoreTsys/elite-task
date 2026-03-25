using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Elite.Common.Utilities.ExceptionHandling;
using Elite.Common.Utilities.HttpClientHelper;
using Elite.Common.Utilities.RequestContext;
using Elite.Task.Microservice.Application.CQRS.IntegrationEvents.Events;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Elite.Common.Utilities.CommonType;
using Elite.Common.Utilities.Encription;

namespace Elite_Task.Microservice.Application.CQRS.ExternalService
{
    public class MeetingTaskService : IMeetingTaskService
    {
        #region Consts
        const string MEETINGSERVICEBASEURL = "MeetingService:BaseUrl";
        const string MEETINGSERVICEAPILINKMEETINGS = "MeetingService:ApiLink:Meetings";
        #endregion
        protected readonly IConfiguration _configuration;
        private HttpClientHelper _httpHelper;
        private IRequestContext _context;

        public MeetingTaskService(IConfiguration configuration,IRequestContext context)
        {
            this._configuration = configuration;
            this._context = context;
            //this._httpHelper = new HttpClientHelper(_configuration.GetSection("MeetingService:BaseUrl").Value, context.tokenInfo, context.UID);
        }

        public async Task PublishDeleteMeetingTaskThroughEventBusAsync(MeetingMinuteTaskDeleteEvent evt)
        {
            this._httpHelper = new HttpClientHelper(_configuration.GetSection("MeetingService:BaseUrl").Value,false);
            this._httpHelper.SetRequestHeaderForSecureID(_context);
            var contentTopic = new StringContent(JsonConvert.SerializeObject(evt), UTF8Encoding.UTF8, "application/json");
            var topicPostResponse = await _httpHelper.HttpClient.PostAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("MeetingService:ApiLink:MinuteTasksDelete").Value, contentTopic);
            if ((int)topicPostResponse.StatusCode != (int)System.Net.HttpStatusCode.OK)
                throw new EliteException($" Api call was failed { string.Join('/', _configuration.GetSection("MeetingService:BaseUrl").Value, _configuration.GetSection("MeetingService:ApiLink:MinuteTasksDelete").Value, contentTopic)}  with status code - {((int)topicPostResponse.StatusCode)} ");
        }

        public async Task PublishMeetingTaskThroughEventBusAsync(MeetingTaskEvent evt)
        {
            await Post(evt);
        }

        private async Task Post(MeetingTaskEvent evt)
        {
            this._httpHelper = new HttpClientHelper(_configuration.GetSection("MeetingService:BaseUrl").Value, false);
            this._httpHelper.SetRequestHeaderForSecureID(_context);
            HttpResponseMessage topicPostResponse = null;
            var data = evt.Task;
            var contentTopic = new StringContent(JsonConvert.SerializeObject(data), UTF8Encoding.UTF8, "application/json");

            if (evt.RequestId > 0)
                topicPostResponse = await _httpHelper.HttpClient.PutAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("MeetingService:ApiLink:MinuteTasks").Value, contentTopic);
            else
                topicPostResponse = await _httpHelper.HttpClient.PostAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("MeetingService:ApiLink:MinuteTasks").Value, contentTopic);

            if ((int)topicPostResponse.StatusCode != (int)System.Net.HttpStatusCode.OK)
                throw new EliteException($" Api call was failed { string.Join('/', _configuration.GetSection("MeetingService:BaseUrl").Value, _configuration.GetSection("MeetingService:ApiLink:MinuteTasks").Value)}  with status code - {((int)topicPostResponse.StatusCode)} ");
        }


		public async Task PublishMeetingAgendaThroughEventBusAsync(MeetingAgendaEvent evt)
		{
			await PostMeetingAgenda(evt);
		}

		private async Task PostMeetingAgenda(MeetingAgendaEvent evt)
		{
			this._httpHelper = new HttpClientHelper(_configuration.GetSection("MeetingService:BaseUrl").Value, false);
            this._httpHelper.SetRequestHeaderForSecureID(_context);
			HttpResponseMessage topicPostResponse = null;
			var data = evt;
			var contentTopic = new StringContent(JsonConvert.SerializeObject(data), UTF8Encoding.UTF8, "application/json");

			if (evt.agendaId > 0 && evt.meetingId > 0)
				topicPostResponse = await _httpHelper.HttpClient.PutAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("MeetingService:ApiLink:MeetingAgendaUpdate").Value, contentTopic);
			
			if ((int)topicPostResponse.StatusCode != (int)System.Net.HttpStatusCode.OK)
				throw new EliteException($" Api call was failed { string.Join('/', _configuration.GetSection("MeetingService:BaseUrl").Value, _configuration.GetSection("MeetingService:ApiLink:MeetingAgendaUpdate").Value)}  with status code - {((int)topicPostResponse.StatusCode)} ");
		}

		public async Task<long> GetTopicId(long id)
        {
            this._httpHelper = new HttpClientHelper(_configuration.GetSection("MeetingService:BaseUrl").Value, false);
            this._httpHelper.SetRequestHeaderForSecureID(_context);
            var topicGetResponse = await _httpHelper.HttpClient.GetAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("MeetingService:ApiLink:GetTopicId").Value + id);

            if ((int)topicGetResponse.StatusCode == (int)System.Net.HttpStatusCode.OK)
            {
                var topicid = await topicGetResponse.Content.ReadAsStringAsync();
                return Convert.ToInt64(topicid);

            }
            else
                throw new EliteException($" Api call was failed { string.Join('/', _configuration.GetSection("MeetingService:BaseUrl").Value, _configuration.GetSection("MeetingService:ApiLink:GetTopicId").Value)}  with status code - {((int)topicGetResponse.StatusCode)} ");
        }

        public async Task<string> GetAgendaById(long id)
		{
			this._httpHelper = new HttpClientHelper(_configuration.GetSection("MeetingService:BaseUrl").Value, false);
            this._httpHelper.SetRequestHeaderForSecureID(_context);
			var topicGetResponse = await _httpHelper.HttpClient.GetAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("MeetingService:ApiLink:MeetingAgenda").Value + id);

			if ((int)topicGetResponse.StatusCode == (int)System.Net.HttpStatusCode.OK)
			{
				var topicResponsibles = await topicGetResponse.Content.ReadAsStringAsync();
				return topicResponsibles;

			}
			else
				throw new EliteException($" Api call was failed { string.Join('/', _configuration.GetSection("MeetingService:BaseUrl").Value, _configuration.GetSection("MeetingService:ApiLink:MeetingAgenda").Value)}  with status code - {((int)topicGetResponse.StatusCode)} ");
		}

		public async Task<bool> GetMeetingAsync(long meetingid)
        {
            this._httpHelper = new HttpClientHelper(_configuration.GetSection("MeetingService:BaseUrl").Value, false);
            this._httpHelper.SetRequestHeaderForSecureID(_context);
            var topicGetResponse = await _httpHelper.HttpClient.GetAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("MeetingService:ApiLink:GetMeeting").Value + meetingid);

            if ((int)topicGetResponse.StatusCode == (int)System.Net.HttpStatusCode.OK)
            {
                var meeting = JsonConvert.DeserializeObject<dynamic>(await topicGetResponse.Content.ReadAsStringAsync());
                if (meeting != null)
                    return meeting.isFinalMinutesTasks;
            }
            throw new EliteException($" Api call was failed { string.Join('/', _configuration.GetSection("MeetingService:BaseUrl").Value, _configuration.GetSection("MeetingService:ApiLink:GetMeeting").Value)}  with status code - {((int)topicGetResponse.StatusCode)} ");
        }

        public async Task<Elite.Common.Utilities.CommonType.MeetingInfo> GetMeetingInfo(long id,long taskid)
        {
            this._httpHelper = new HttpClientHelper(_configuration.GetSection(MEETINGSERVICEBASEURL).Value, false);
            this._httpHelper.SetRequestHeaderForSecureID(_context);
            var userGetResponse = await _httpHelper.HttpClient.GetAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) 
                 + _configuration.GetSection(MEETINGSERVICEAPILINKMEETINGS).Value + id+ "&taskid="+taskid);

            if ((int)userGetResponse.StatusCode == (int)System.Net.HttpStatusCode.OK)
            {
                var response = await userGetResponse.Content.ReadAsStringAsync();
                var meetinginfo = JsonConvert.DeserializeObject<Elite.Common.Utilities.CommonType.MeetingInfo >(response);
                return meetinginfo;
            }
            return null;
        }
        public async Task<string> GetAgendaTitle(long id)
        {
            this._httpHelper = new HttpClientHelper(_configuration.GetSection("MeetingService:BaseUrl").Value, false);
            this._httpHelper.SetRequestHeaderForSecureID(_context);
            var topicGetResponse = await _httpHelper.HttpClient.GetAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("MeetingService:ApiLink:GetAgendaTitle").Value + id);

            if ((int)topicGetResponse.StatusCode == (int)System.Net.HttpStatusCode.OK)
            {
                var topicid = await topicGetResponse.Content.ReadAsStringAsync();
                return topicid;

            }
            else
                throw new EliteException($" Api call was failed {string.Join('/', _configuration.GetSection("MeetingService:BaseUrl").Value, _configuration.GetSection("MeetingService:ApiLink:GetAgendaTitle").Value)}  with status code - {((int)topicGetResponse.StatusCode)} ");
        }

    }
}
