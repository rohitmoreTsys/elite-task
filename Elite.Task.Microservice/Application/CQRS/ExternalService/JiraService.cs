using Elite.Common.Utilities.Encription;
using Elite.Common.Utilities.ExceptionHandling;
using Elite.Common.Utilities.HttpClientHelper;
using Elite.Common.Utilities.JiraEntities;
using Elite.Common.Utilities.RequestContext;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.CQRS.ExternalService
{
    public class JiraService : IJiraService
    {
        protected readonly IConfiguration _configuration;
        private HttpClientHelper _httpHelper;
        private IRequestContext _context;
        public JiraService(IConfiguration configuration, IRequestContext context)
        {
            this._configuration = configuration;
            this._context = context;

        }
        public async Task<JiraTicketResponse> CreateTaskInJira(string jiraTicketData)
        {
            setClientHelper();
            var taskContent = new StringContent(jiraTicketData, UTF8Encoding.UTF8, "application/json");
            var taskCreateResponse = await _httpHelper.HttpClient.PostAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("JiraService:ApiLink:JiraTaskApi").Value, taskContent);
            if ((int)taskCreateResponse.StatusCode == (int)System.Net.HttpStatusCode.Created)
            {

                var result = JsonConvert.DeserializeObject<JiraTicketResponse>(await taskCreateResponse.Content.ReadAsStringAsync());
                return result;
            }
            throw new EliteException($" Api call has failed {taskCreateResponse.ReasonPhrase} { string.Join('/', _configuration.GetSection("JiraService:BaseUrl").Value, _configuration.GetSection("JiraService:ApiLink:JiraTaskApi").Value)}  with status code - {((int)taskCreateResponse.StatusCode)} ");
        }

        public async Task<string> UpdateTaskInJira(string jiraTicketData)
        {
            setClientHelper();
            var taskContent = new StringContent(jiraTicketData, UTF8Encoding.UTF8, "application/json");
            var taskUpdateResponse = await _httpHelper.HttpClient.PutAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("JiraService:ApiLink:JiraTaskApi").Value, taskContent);
            if ((int)taskUpdateResponse.StatusCode == (int)System.Net.HttpStatusCode.NoContent)
            {

                var response = await taskUpdateResponse.Content.ReadAsStringAsync();
                return response;
            }
            throw new EliteException($" Api call has failed { string.Join('/', _configuration.GetSection("JiraService:BaseUrl").Value, _configuration.GetSection("JiraService:ApiLink:JiraTaskApi").Value)}  with status code - {((int)taskUpdateResponse.StatusCode)} ");
        }

        public async Task<TaskInfoFromJira> GetTaskFromJira(string jiraTicketId)
        {
            setClientHelper();
            var taskUpdateResponse = await _httpHelper.HttpClient.GetAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("JiraService:ApiLink:JiraTaskApi").Value + "/" + jiraTicketId);
            if ((int)taskUpdateResponse.StatusCode == (int)System.Net.HttpStatusCode.OK)
            {

                var response = await taskUpdateResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TaskInfoFromJira>(response);
            }
            throw new EliteException($" Api call has failed { string.Join('/', _configuration.GetSection("JiraService:BaseUrl").Value, _configuration.GetSection("JiraService:ApiLink:JiraTaskApi").Value)}  with status code - {((int)taskUpdateResponse.StatusCode)} ");
        }


        public async Task<string> SetTaskStatusToDeleteInJira(string jiraTicketKey)
        {
            setClientHelper();
            var taskUpdateResponse = await _httpHelper.HttpClient.DeleteAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("JiraService:ApiLink:JiraTaskApi").Value + "/" + jiraTicketKey);
            if ((int)taskUpdateResponse.StatusCode == (int)System.Net.HttpStatusCode.NoContent)
            {
                var response = await taskUpdateResponse.Content.ReadAsStringAsync();
                return response;
            }
            throw new EliteException($" Api call has failed { string.Join('/', _configuration.GetSection("JiraService:BaseUrl").Value, _configuration.GetSection("JiraService:ApiLink:JiraTaskApi").Value)}  with status code - {((int)taskUpdateResponse.StatusCode)} ");
        }

        public async Task<bool> CheckJiraTokenValidity()
        {
            setClientHelper();
            var userGetResponse = await _httpHelper.HttpClient.GetAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("JiraService:ApiLink:JiraAccessApi").Value);
            if ((int)userGetResponse.StatusCode == (int)System.Net.HttpStatusCode.OK)
            {
                var response = await userGetResponse.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<bool>(response);
                return result;
            }
            throw new EliteException($" Api call has failed { string.Join('/', _configuration.GetSection("JiraService:BaseUrl").Value, _configuration.GetSection("JiraService:ApiLink:JiraAccessApi").Value)}  with status code - {((int)userGetResponse.StatusCode)} ");
        }

        private void setClientHelper()
        {
            if (_context.IsHttpContextExist)
            {
                this._httpHelper = new HttpClientHelper(_configuration.GetSection("JiraService:BaseUrl").Value, false);
                this._httpHelper.SetRequestHeader(_configuration, _context.UID, _context.DeputyUID);
            }
            else
            {
                this._httpHelper = new HttpClientHelper(_configuration.GetSection("JiraService:BaseUrl").Value, false);

            }

        }
    }
}
