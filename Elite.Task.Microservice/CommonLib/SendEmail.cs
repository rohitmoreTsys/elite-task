using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.CommonLib
{
    public class SendEmail
    {
        public SendEmail()
        { Receipients = new List<string>(); }
        public string Subject { get; set; }
        public string Body { get; set; }
        public byte[] Attachment { get; set; }
        public string AttachmentFileName { get; set; }
        public IList<string> Receipients { get; set; }
        public int taskType { get; set;}
        public string basePath { get; set; }
        public SendEmailType SMTPEmailwithTemplate { get; set; }
        //public IList<string> BCCRecipients { get; set; }
        //public IList<string> CCRecipients { get; set; }
        //public AlternateView alternateView { get; set; }
    }
}
