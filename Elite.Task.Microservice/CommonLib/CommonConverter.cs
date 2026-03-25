using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite_Task.Microservice.CommonLib
{
    public class CommonConverter
    {
        public static DateTime ConvertDateFromITC(DateTime dateTime, IConfiguration _configuration)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.FindSystemTimeZoneById(_configuration.GetSection("TimeZone:IST").Value));
        }

        public static DateTimeOffset ConvertTimeFromITC(DateTime dateTime, IConfiguration _configuration)
        {
            return new DateTimeOffset(dateTime.Year,dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, TimeZoneInfo.FindSystemTimeZoneById(_configuration.GetSection("TimeZone:IST").Value).GetUtcOffset(DateTime.Now));
        }
    }
}
