using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.NotificationServices
{

    public class HTMLReadUtility : IHTMLReadUtility
    {

        private string _taskResponsible;
        private string _taskCompletion;
        private string _taskReopen;
        private string _taskDelete;
        private string _taskResponsibleChanged;
        private string _taskstatus;
        private string _taskCompleted;
        private string _taskRejected;
        private string _taskCoResponsible;
        private string _taskCoResponsibleChanged;
        private string _meetingTaskResponsible;
        private string _meetingTaskResponsibleBoM;
        private string _meetingTaskCoResponsible;
        private string _meetingTaskCoResponsibleBoM;
        private string _taskUpdate;

        protected readonly IConfiguration _configuration;
        private IHostingEnvironment _env;

        public string TaskResponsible => string.Copy(_taskResponsible);
        public string TaskDelete => string.Copy(_taskDelete);
        public string TaskResponsibleChanged => string.Copy(_taskResponsibleChanged);
        public string TaskStatus => string.Copy(_taskstatus);
        public string TaskCompleted => string.Copy(_taskCompleted);
        public string TaskRejected => string.Copy(_taskRejected);
        public string TaskCoResponsible => string.Copy(_taskCoResponsible);
        public string TaskCoResponsibleChanged => string.Copy(_taskCoResponsibleChanged);
        public string MeetingTaskResponsible => string.Copy(_meetingTaskResponsible);
        public string MeetingTaskResponsibleBoM => string.Copy(_meetingTaskResponsibleBoM);
        public string MeetingTaskCoResponsible => string.Copy(_meetingTaskCoResponsible);
        public string MeetingTaskCoResponsibleBoM => string.Copy(_meetingTaskCoResponsibleBoM);
        public string TaskUpdate => string.Copy(_taskUpdate);

        public HTMLReadUtility(IConfiguration configuration, IHostingEnvironment env)
        {
            string fileResponsible;
            string fileDelete;
            string fileResponsibleChanged;
            string fileCoResponsible;
            string fileCoResponsibleChanged;
            string fileMeetingTaskCoResponsible;
            string fileMeetingTaskCoResponsibleBoM;
            string fileMeetingTaskResponsible;
            string fileMeetingTaskResponsibleBoM;
            string fileStatus;
            string fileCompleted;
            string fileRejected;
            string fileUpdate;

            this._configuration = configuration;
            _env = env;
            var webRoot = _env.ContentRootPath;

            fileResponsible = System.IO.Path.Combine(webRoot, _configuration.GetSection("EmailNotification:TaskResponsible:FileName").Value);
            fileDelete = System.IO.Path.Combine(webRoot, _configuration.GetSection("EmailNotification:TaskDelete:FileName").Value);
            fileStatus = System.IO.Path.Combine(webRoot, _configuration.GetSection("EmailNotification:TaskStatus:FileName").Value);
            fileResponsibleChanged = System.IO.Path.Combine(webRoot, _configuration.GetSection("EmailNotification:TaskResponsibleChanged:FileName").Value);
            fileCoResponsible = System.IO.Path.Combine(webRoot, _configuration.GetSection("EmailNotification:TaskCoResponsible:FileName").Value);
            fileCoResponsibleChanged = System.IO.Path.Combine(webRoot, _configuration.GetSection("EmailNotification:TaskCoResponsibleChanged:FileName").Value);
            fileMeetingTaskCoResponsible= System.IO.Path.Combine(webRoot, _configuration.GetSection("EmailNotification:TaskForMeetingCoResponsible:FileName").Value);
            fileMeetingTaskResponsible= System.IO.Path.Combine(webRoot, _configuration.GetSection("EmailNotification:TaskForMeetingResponsible:FileName").Value);
            fileMeetingTaskResponsibleBoM= System.IO.Path.Combine(webRoot, _configuration.GetSection("EmailNotification:TaskForMeetingResponsibleBoM:FileName").Value);
            fileMeetingTaskCoResponsibleBoM = System.IO.Path.Combine(webRoot, _configuration.GetSection("EmailNotification:TaskForMeetingCoResponsibleBoM:FileName").Value);
            fileCompleted = System.IO.Path.Combine(webRoot, _configuration.GetSection("EmailNotification:TaskCompleted:FileName").Value);
            fileRejected = System.IO.Path.Combine(webRoot, _configuration.GetSection("EmailNotification:TaskRejected:FileName").Value);
            fileUpdate = System.IO.Path.Combine(webRoot, _configuration.GetSection("EmailNotification:TaskUpdate:FileName").Value);

            if (System.IO.File.Exists(fileResponsible))
                _taskResponsible = File.ReadAllText(fileResponsible);
            else
                _taskResponsible = string.Empty;

            if (System.IO.File.Exists(fileDelete))
                _taskDelete = System.IO.File.ReadAllText(fileDelete);
            else
                _taskDelete = string.Empty;

            if (System.IO.File.Exists(fileResponsibleChanged))
                _taskResponsibleChanged = System.IO.File.ReadAllText(fileResponsibleChanged);
            else
                _taskResponsibleChanged = string.Empty;

            if (System.IO.File.Exists(fileCoResponsible))
                _taskCoResponsible = System.IO.File.ReadAllText(fileCoResponsible);
            else
                _taskCoResponsible = string.Empty;

            if (System.IO.File.Exists(fileCoResponsibleChanged))
                _taskCoResponsibleChanged = System.IO.File.ReadAllText(fileCoResponsibleChanged);
            else
                _taskCoResponsibleChanged = string.Empty;

            if (System.IO.File.Exists(fileStatus))
                _taskstatus = System.IO.File.ReadAllText(fileStatus);
            else
                _taskstatus = string.Empty;

            if (System.IO.File.Exists(fileCompleted))
                _taskCompleted = System.IO.File.ReadAllText(fileCompleted);
            else
                _taskCompleted = string.Empty;

            if (System.IO.File.Exists(fileRejected))
                _taskRejected = System.IO.File.ReadAllText(fileRejected);
            else
                _taskRejected = string.Empty;


            if (System.IO.File.Exists(fileMeetingTaskResponsible))
                _meetingTaskResponsible = System.IO.File.ReadAllText(fileMeetingTaskResponsible);
            else
                _meetingTaskResponsible = string.Empty;

            if (System.IO.File.Exists(fileMeetingTaskResponsibleBoM))
                _meetingTaskResponsibleBoM = System.IO.File.ReadAllText(fileMeetingTaskResponsibleBoM);
            else
                _meetingTaskResponsibleBoM = string.Empty;

            if (System.IO.File.Exists(fileMeetingTaskCoResponsible))
                _meetingTaskCoResponsible = System.IO.File.ReadAllText(fileMeetingTaskCoResponsible);
            else
                _meetingTaskCoResponsible = string.Empty;

            if (System.IO.File.Exists(fileMeetingTaskCoResponsibleBoM))
                _meetingTaskCoResponsibleBoM = System.IO.File.ReadAllText(fileMeetingTaskCoResponsibleBoM);
            else
                _meetingTaskCoResponsibleBoM = string.Empty;

            if (System.IO.File.Exists(fileUpdate))
                _taskUpdate = File.ReadAllText(fileUpdate);
            else
                _taskUpdate = string.Empty;

        }
    }
}
