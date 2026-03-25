using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite_Task.Microservice.Application.Paging
{
    public class PageHelper
    {       
        public static int GetTopicListPageSize(int listType, IConfiguration _configuration)
        {
            int pageSize = (listType == (int)PageType.Tile) ? Convert.ToInt32(_configuration.GetSection("TopicTilePageSize").Value) : Convert.ToInt32(_configuration.GetSection("TopicListPageSize").Value);
            return pageSize;
        }

        public static int GetMeetingTopicListPageSize(IConfiguration _configuration)
        {
            return  Convert.ToInt32(_configuration.GetSection("MeetingTopicListPageSize").Value);
        }
    }
}
