using Elite.Common.Utilities.CommonType;
using Elite.Common.Utilities.Encription;
using Elite.Common.Utilities.ExceptionHandling;
using Elite.Common.Utilities.HttpClientHelper;
using Elite.Common.Utilities.RequestContext;
using Elite.Task.Microservice.Application.CQRS.Helpers;
using Elite.Task.Microservice.Application.CQRS.Queries.QueriesDto;
using Elite.Task.Microservice.CommonLib;
using Elite_Task.Microservice.Application.CQRS.Commands;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.CQRS.ExternalService
{
    public class UserService : IUserService
    {
        protected readonly IConfiguration _configuration;
        private HttpClientHelper _httpHelper;
        private IRequestContext _context;
        public UserService(IConfiguration configuration, IRequestContext context)
        {
            this._configuration = configuration;
            this._context = context;

            //if (context.ContextAccessor.HttpContext!=null)
            //{
            //    this._httpHelper = new HttpClientHelper(_configuration.GetSection("UserService:BaseUrl").Value, _context.tokenInfo, _context.UID);
            //}
            //else
            //{
            //    this._httpHelper = new HttpClientHelper(_configuration.GetSection("UserService:BaseUrl").Value, "", "");
            //}
        }
        private void setClientHelper()
        {
            if (_context.IsHttpContextExist)
            {
                this._httpHelper = new HttpClientHelper(_configuration.GetSection("UserService:BaseUrl").Value, false);
                this._httpHelper.SetRequestHeaderForSecureID(_context);
                this._httpHelper.SetRequestHeader(_configuration, _context.UID, _context.DeputyUID);
            }
            else
            {
                this._httpHelper = new HttpClientHelper(_configuration.GetSection("UserService:BaseUrl").Value, false);

            }

        }

        public async Task<UidEmail> GetUserEmailId(string id)
        {
            setClientHelper();
            // this._httpHelper = new HttpClientHelper(_configuration.GetSection("UserService:BaseUrl").Value, _context.tokenInfo, _context.UID);
            var topicGetResponse = await _httpHelper.HttpClient.GetAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("UserService:ApiLink:UserEmail").Value + id);

            if ((int)topicGetResponse.StatusCode == (int)System.Net.HttpStatusCode.OK)
            {
                var users = JsonConvert.DeserializeObject<UidEmail>(await topicGetResponse.Content.ReadAsStringAsync());
                if (users != null)
                    return users;
            }
            return null;
        }

        /// <summary>
        /// Get the UserID and DisplayName
        /// </summary>
        /// Rahul Kumar on 03-05-2019   
        public async Task<TaskPersonCommand> GetUser(string uid)
        {
            setClientHelper();
            // this._httpHelper = new HttpClientHelper(_configuration.GetSection("UserService:BaseUrl").Value, _context.tokenInfo, _context.UID);
            var topicGetResponse = await _httpHelper.HttpClient.GetAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("UserService:ApiLink:GetUser").Value + uid);

            if ((int)topicGetResponse.StatusCode == (int)System.Net.HttpStatusCode.OK)
            {
                TaskPersonCommand user = JsonConvert.DeserializeObject<TaskPersonCommand>(await topicGetResponse.Content.ReadAsStringAsync());
                if (user != null)
                    return new TaskPersonCommand(user.Uid, user.DisplayName);
            }
            throw new EliteException($" Api call was failed { string.Join('/', _configuration.GetSection("UserService:BaseUrl").Value, _configuration.GetSection("UserService:ApiLink:GetUser").Value + uid)}  with status code - {((int)topicGetResponse.StatusCode)} ");
        }

        public async Task<CommitteeManagersMailIdsDto> GetCommitteeManagersMailIds(long id, string requestorUid, string createdByUid)
        {
            this._httpHelper = new HttpClientHelper(_configuration.GetSection("UserService:BaseUrl").Value, false);
            this._httpHelper.SetRequestHeader(this._configuration, this._context.UID, _context.DeputyUID is null ? _context.UID : _context.DeputyUID);
            this._httpHelper.SetRequestHeaderForMailsIds(this._configuration, requestorUid, createdByUid);
            var topicGetResponse = await _httpHelper.HttpClient.GetAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("UserService:ApiLink:GetCommitteeManagersMailIds").Value + id);

            if ((int)topicGetResponse.StatusCode == (int)System.Net.HttpStatusCode.OK)
            {
                var users = JsonConvert.DeserializeObject<CommitteeManagersMailIdsDto>(await topicGetResponse.Content.ReadAsStringAsync());
                if (users != null)
                    return users;
            }
            return new CommitteeManagersMailIdsDto();
        }

        //Get user detail for createdby or modified by
        public LookUpFields GetUserDetail(string securedUID)
        {
            Users user = null;
            user = GetUserByUid(securedUID).GetAwaiter().GetResult();

            if (user != null && user.Uid != null)
            {
                return new LookUpFields
                {
                    Uid = user.Uid.ToUpper(),
                    DisplayName = user.DisplayName,
                    FullName = $"{user.Title} {user.FirstName} {user.LastName}".Trim(),
                    Title = user.Title
                };
            }
            else
                throw new NullReferenceException();
        }

        private async Task<Users> GetUserByUid(string uid)
        {
            Users user = new Users();
            setClientHelper();
            var topicGetResponse = await _httpHelper.HttpClient.GetAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("UserService:ApiLink:GetUser").Value + uid);
            if ((int)topicGetResponse.StatusCode == (int)System.Net.HttpStatusCode.OK)
            {
                string result = await topicGetResponse.Content.ReadAsStringAsync();
                user = JsonConvert.DeserializeObject<Users>(result);
                if (user != null)
                    return user;
            }
            throw new EliteException($" Api call was failed {string.Join('/', _configuration.GetSection("UserService:BaseUrl").Value, _configuration.GetSection("UserService:ApiLink:GetUser").Value + uid)}  with status code - {((int)topicGetResponse.StatusCode)} ");
        }


        public async Task<IList<UserRolesAndRights>> GetUserRolesAndRights(string id)
        {
            setClientHelper();

            //  this._httpHelper = new HttpClientHelper(_configuration.GetSection("UserService:BaseUrl").Value, _context.tokenInfo, _context.UID);
            var topicGetResponse = await _httpHelper.HttpClient.GetAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("UserService:ApiLink:GetUserRolesAndRights").Value + id);

            if ((int)topicGetResponse.StatusCode == (int)System.Net.HttpStatusCode.OK)
            {
                var users = JsonConvert.DeserializeObject<List<UserRolesAndRights>>(await topicGetResponse.Content.ReadAsStringAsync());
                if (users?.Count > 0)
                    return users;
            }
            return new List<UserRolesAndRights>();
        }

        public async Task<List<CommitteeDetail>> GetCommitees()
        {
            setClientHelper();
            //  this._httpHelper = new HttpClientHelper(_configuration.GetSection("UserService:BaseUrl").Value, _context.tokenInfo, _context.UID);
            var topicGetResponse = await _httpHelper.HttpClient.GetAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("UserService:ApiLink:GetCommitteesDetail").Value);

            if ((int)topicGetResponse.StatusCode == (int)System.Net.HttpStatusCode.OK)
            {
                var committees = JsonConvert.DeserializeObject<List<CommitteeDetail>>(await topicGetResponse.Content.ReadAsStringAsync());
                if (committees?.Count > 0)
                    return committees;
            }
            return new List<CommitteeDetail>();
        }

        public async Task<List<LookUp>> GetUserCommittees()
        {
            setClientHelper();
            var topicGetResponse = await _httpHelper.HttpClient.GetAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("UserService:ApiLink:GetUserCommittees").Value);

            if ((int)topicGetResponse.StatusCode == (int)System.Net.HttpStatusCode.OK)
            {
                var committees = JsonConvert.DeserializeObject<List<LookUp>>(await topicGetResponse.Content.ReadAsStringAsync());
                if (committees?.Count > 0)
                    return committees;
            }
            return new List<LookUp>();
        }
        public async Task<TaskPersonCommand> ValidateAndPostUsers(TaskPersonCommand jiraResponsible)
        {
            setClientHelper();

            var json = JsonConvert.SerializeObject(jiraResponsible);
            var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
            var jiraGetUser = await _httpHelper.HttpClient.PostAsync(_httpHelper.HttpClient.BaseAddress.ToString() + _configuration.GetSection("UserService:ApiLink:ValidateAndPostUsers").Value, stringContent);

            if ((int)jiraGetUser.StatusCode == (int)System.Net.HttpStatusCode.OK)
            {
                TaskPersonCommand user = JsonConvert.DeserializeObject<TaskPersonCommand>(await jiraGetUser.Content.ReadAsStringAsync());
                if (user != null)
                    return new TaskPersonCommand(user.Uid, user.DisplayName);
            }
            throw new EliteException($" Api call was failed { string.Join('/', _configuration.GetSection("UserService:BaseUrl").Value, _configuration.GetSection("UserService:ApiLink:ValidateAndPostUsers").Value + jiraResponsible.Uid + jiraResponsible.DisplayName)}  with status code - {((int)jiraGetUser.StatusCode)} ");
        }

        public async Task<UserInfo> GetUserInfos(string id)
        {
            this._httpHelper = new HttpClientHelper(_configuration.GetSection("UserService:BaseUrl").Value, false);
            this._httpHelper.SetRequestHeaderForSecureID(_context);
            var topicGetResponse = await _httpHelper.HttpClient.GetAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("UserService:ApiLink:GetUserInfos").Value + id);
            if ((int)topicGetResponse.StatusCode == (int)System.Net.HttpStatusCode.OK)
            {
                var users = JsonConvert.DeserializeObject<UserInfo>(await topicGetResponse.Content.ReadAsStringAsync());
                if (users.FirstName != "" && users.LastName != "")
                    return users;
            }


            return new UserInfo();
        }

        public async Task<List<UserDeputies>> GetRestrictedUserDeputies(string userId)
        {
            //_configuration.GetSection("UserService:ApiLink:GetUserInfos").Value 
            this._httpHelper = new HttpClientHelper(_configuration.GetSection("UserService:BaseUrl").Value, false);
            this._httpHelper.SetRequestHeaderForSecureID(_context);
            var topicGetResponse = await _httpHelper.HttpClient.GetAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + "api/AssigningDeputy/GetRestrictedUserDeputies/" + userId);
            if ((int)topicGetResponse.StatusCode == (int)System.Net.HttpStatusCode.OK)
            {
                var userDeputies = JsonConvert.DeserializeObject<List<UserDeputies>>(await topicGetResponse.Content.ReadAsStringAsync());
                return userDeputies;
            }
            return null;
        }
        public async Task<List<LookUp>> GetListofUsersCommitteeManagersCommitteeAsync(string uid)
        {
            this._httpHelper = new HttpClientHelper(_configuration.GetSection("AdminService:BaseUrl").Value, false);
            this._httpHelper.SetRequestHeaderForSecureID(_context);
            var response = await _httpHelper.HttpClient.GetAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("AdminService:ApiLink:GetAllCMasUserCommitteID").Value + uid);
            if ((int)response.StatusCode == (int)System.Net.HttpStatusCode.OK)
            {
                var listofUserCommiteeasManager = JsonConvert.DeserializeObject<List<LookUp>>(await response.Content.ReadAsStringAsync());
                if (listofUserCommiteeasManager.Count > 0)
                    return listofUserCommiteeasManager;
            }
            return new List<LookUp>();
        }
    }
}
