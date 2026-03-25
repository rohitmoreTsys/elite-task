using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Elite.Common.Utilities.HttpClientHelper;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading;

namespace Elite.Common.Utilities.Attachment.Delete.MapOrphans
{
    public class AttachmentService : IAttachmentService
    {
        protected readonly IConfiguration _configuration;
        private HttpClientHelper.HttpClientHelper _httpHelper;

        public AttachmentService(IConfiguration configuration)
        {
            this._configuration = configuration;
            this._httpHelper = new HttpClientHelper.HttpClientHelper(_configuration.GetSection("AttachmentService:BaseUrl").Value);
        }

        public void PublishThroughEventBusForDelete(AttachmentDeleteOrMappingEvent evt) => PostDelete(evt);       

        public void PublishThroughEventBusForMapping(AttachmentDeleteOrMappingEvent evt) => PostMapping(evt);

        /// <summary>
        /// Deleting the attachment from attachment service, It’s a asynchronous process
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        private async void PostDelete(AttachmentDeleteOrMappingEvent evt)
        {          
            var contentTopic = new StringContent(JsonConvert.SerializeObject(evt), UTF8Encoding.UTF8, "application/json");
            var topicPostResponse = await _httpHelper.HttpClient.PostAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("AttachmentService:ApiLink:Delete").Value, contentTopic);          

            /*
               if ((int)topicPostResponse.StatusCode == (int)System.Net.HttpStatusCode.OK)                
                     //logging Not Implemented
                else
                     //Exception handling is still pending
                     //Need to throw Explicit exception 
                     //logging Not Implemented
             */
        }

        /// <summary>
        /// Mapping attachment to attachment service, It’s a asynchronous process
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        private async void PostMapping(AttachmentDeleteOrMappingEvent evt)
        {
            var contentTopic = new StringContent(JsonConvert.SerializeObject(evt), UTF8Encoding.UTF8, "application/json");         
                var topicPostResponse = await _httpHelper.HttpClient.PostAsync(new Uri(_httpHelper.HttpClient.BaseAddress.ToString()) + _configuration.GetSection("AttachmentService:ApiLink:Update").Value, contentTopic);
            /*
              if ((int)topicPostResponse.StatusCode == (int)System.Net.HttpStatusCode.OK)                
                    //logging Not Implemented
               else
                    //Exception handling is still pending
                    //Need to throw Explicit exception 
                    //logging Not Implemented
            */
        }
    }
}
