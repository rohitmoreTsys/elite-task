using Elite.Task.Microservice.CommonLib;
using MediatR;
using Microsoft.Extensions.Configuration;
using Polly.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.NotificationServices
{
    public class EmailTemplatesQueryHandler : IRequestHandler<EmailTemplatesQuery, TaskEmailTemplates>
    {

        private readonly IHTMLReadUtility _readHTML;
        private readonly IMediator _mediator;
        protected readonly IConfiguration _configuration;



        #region Const for meeting Email
        private const string EMAILNOTIFICATIONSTATUS = "EmailNotification:TokenReplacement:STATUS";
        private const string EMAILNOTIFICATIONPERSON = "EmailNotification:TokenReplacement:PERSON";
        private const string EMAILNOTIFICATIONTASKLINK = "EmailNotification:TokenReplacement:TASKLINK";
        private const string EMAILNOTIFICATIONTASK = "EmailNotification:TokenReplacement:TASK";
        private const string EMAILNOTIFICATIONDEMO = "EmailNotification:TokenReplacement:DEMO";
        private const string EMAILNOTIFICATIONDUEDATE = "EmailNotification:TokenReplacement:DUEDATE";
        private const string EMAILNOTIFICATIONCOMMITTEE = "EmailNotification:TokenReplacement:COMMITTEE";
        private const string EMAILNOTIFICATIONROLE = "EmailNotification:TokenReplacement:ROLE";
        private const string EMAILNOTIFICATIONTITLE = "EmailNotification:TokenReplacement:TITLE";
        private const string EMAILNOTIFICATIONTO = "EmailNotification:TokenReplacement:TO";
        private const string EMAILNOTIFICATIONMEETINGLINK = "EmailNotification:TokenReplacement:MEETINGLINK";
        private const string EMAILNOTIFICATIONTASKDESCRIPTION = "EmailNotification:TokenReplacement:TASKDESCRIPTION";
        private const string EMAILNOTIFICATIONCLOSURECOMMENT = "EmailNotification:TokenReplacement:CLOSURECOMMENT";
        private const string EMAILNOTIFICATIONMEETINGNAME = "EmailNotification:TokenReplacement:MEETINGNAME";
        private const string EMAILNOTIFICATIONMEETINGACTIONLINK = "EmailNotification:MeetingActionLink";
        private const string EMAILNOTIFICATIONTASKACTIONLINK = "EmailNotification:TaskActionLink";
        private const string EMAILNOTIFICATIONMEETINGDATE = "EmailNotification:TokenReplacement:MEETINGDATE";
        private const string EMAILNOTIFICATIONMEETINGINFO = "EmailNotification:TokenReplacement:MEETINGINFO";
        private const string CREATOREMAIL = "EmailNotification:TokenReplacement:CREATOREMAIL";
        private const string DEMOCONTENTGERMAN = "EmailNotification:TokenReplacement:DEMOCONTENTGERMAN";
        private const string TASKTITLEGERMAN = "EmailNotification:TokenReplacement:TASKTITLEGERMAN";
        private const string TASKDESCRIPTIONGERMAN = "EmailNotification:TokenReplacement:TASKDESCRIPTIONGERMAN";
        private const string DUEDATEGERMAN = "EmailNotification:TokenReplacement:DUEDATEGERMAN";
        private const string CREATORGERMAN = "EmailNotification:TokenReplacement:CREATORGERMAN";
        private const string CREATOREMAILGERMAN = "EmailNotification:TokenReplacement:CREATOREMAILGERMAN";
        private const string TASKACTIONLINKGERMAN = "EmailNotification:TokenReplacement:TASKACTIONLINKGERMAN";
        private const string MEETINGHEADER = "EmailNotification:TokenReplacement:MEETINGHEADER";
        private const string MEETINGDATA = "EmailNotification:TokenReplacement:MEETINGDATA";
        private const string MEETINGLINKINFO = "EmailNotification:TokenReplacement:MEETINGLINKINFO";
        private const string CONFIDENTIAL = "EmailNotification:TokenReplacement:CONFIDENTIAL";
        private const string MEETINGNAMEGERMAN = "EmailNotification:TokenReplacement:MEETINGNAMEGERMAN";
        private const string MEETINGDATEGERMAN = "EmailNotification:TokenReplacement:MEETINGDATEGERMAN";
        private const string MEETINGACTIONLINKGERMAN = "EmailNotification:TokenReplacement:MEETINGACTIONLINKGERMAN";
        private const string MEETINGHEADERGERMAN = "EmailNotification:TokenReplacement:MEETINGHEADERGERMAN";
        private const string MEETINGDATAGERMAN = "EmailNotification:TokenReplacement:MEETINGDATAGERMAN";
        private const string MEETINGLINKINFOGERMAN = "EmailNotification:TokenReplacement:MEETINGLINKINFOGERMAN";
        private const string CONFIDENTIALGERMAN = "EmailNotification:TokenReplacement:CONFIDENTIALGERMAN";
        private const string RESPONSIBLEGERMAN = "EmailNotification:TokenReplacement:RESPONSIBLEGERMAN";
        private const string TASKHEADER = "#TASKHEADER#";
        #endregion
        public EmailTemplatesQueryHandler(IMediator mediator, IHTMLReadUtility readHTML,IConfiguration configuration)
        {
            _mediator = mediator;
            _readHTML = readHTML;
            _configuration = configuration;
        }

        public async Task<TaskEmailTemplates> Handle(EmailTemplatesQuery request, CancellationToken cancellationToken)
        {
            // var ab = new DateTime();

            TaskEmailTemplates taskEmailTemplates = new TaskEmailTemplates();
            string regularTemplate = TokenReplacement(_readHTML.MeetingTaskResponsible, "recipent", "Creteor", "test", "Task Title", "Role", "Committee", "status",

                            false,
                           DateTime.Today,
                            0, "message.Description",
                            "ClosureComment",
                            "meetingName", "MeetingDate",
                            "Meeting Link",
                            0, true, "test@mercedes-benz.com");

            string boMTemplate = TokenReplacement(_readHTML.MeetingTaskResponsibleBoM, "recipent", "Creteor", "test", "Task Title", "Role", "Committee", "status",

                            false,
                           DateTime.Today,
                            0, "message.Description",
                            "ClosureComment",
                            "meetingName", "MeetingDate",
                            "Meeting Link",
                            0, true, "test@mercedes-benz.com");

            taskEmailTemplates.RegularTemplateMeetingTaskResponsible = regularTemplate;
            taskEmailTemplates.BoMTemplateMeetingTaskResponsible = boMTemplate; 
            return taskEmailTemplates;   
        }


        private string TokenReplacement(string source, string to, string person, string link, string title, string role, string committee, string status, bool demouser, DateTime? duedate, long? task, string taskDescription, string closureComment, string meetingName, string meetingDate, string meetingLink, long meetingId, bool isConfidential, string emailAddress)
        {
            string tasknotificationLink = link + "?id=" + task;
            string meetingnotificationLink = link + "?id=" + meetingId;
            string demoEmailContent = string.Empty;
            string meetingLinkInfo = string.Empty;
            string meetingLinkInfoGerman = string.Empty;
            if (demouser)
            {
                demoEmailContent ="";
            }

            string returnResult = string.Empty;

            if (meetingId > 0)
            {
                if ((_configuration.GetSection(EMAILNOTIFICATIONMEETINGACTIONLINK).Value) != null)
                    meetingnotificationLink = ProccessLink(_configuration.GetSection(EMAILNOTIFICATIONMEETINGACTIONLINK).Value, meetingId.ToString());

                meetingLinkInfo = InsertMeetingLink(meetingId, meetingnotificationLink, false);
                meetingLinkInfoGerman = InsertMeetingLink(meetingId, meetingnotificationLink, true);
            }
            string taskHeader = InsertTaskHeader(meetingId, false);
            string taskHeaderGerman = InsertTaskHeader(meetingId, true);
            string taskData = InsertTaskData(title, status, to, meetingId, meetingName, meetingDate);
            string closureCommentElement = string.Empty;
            
            try
            {
                var statusLink = _configuration.GetSection(EMAILNOTIFICATIONSTATUS).Value;
                returnResult = source.Replace(_configuration.GetSection(EMAILNOTIFICATIONTO).Value, to)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONTASKLINK).Value, tasknotificationLink)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONROLE).Value, role)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONCOMMITTEE).Value, committee)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONSTATUS).Value, status)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONDEMO).Value, demoEmailContent)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONTASK).Value,  "Task")
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONCLOSURECOMMENT).Value, closureCommentElement)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONMEETINGLINK).Value, meetingnotificationLink)
                .Replace(_configuration.GetSection(MEETINGHEADER).Value, taskHeader)
                .Replace(_configuration.GetSection(MEETINGDATA).Value, taskData)
                .Replace(_configuration.GetSection(MEETINGLINKINFO).Value, meetingLinkInfo)
                .Replace(_configuration.GetSection(CONFIDENTIAL).Value, isConfidential ? "Confidential" : "")
                .Replace(_configuration.GetSection(DEMOCONTENTGERMAN).Value, demoEmailContent)
                .Replace(_configuration.GetSection(TASKACTIONLINKGERMAN).Value, tasknotificationLink)
                .Replace(_configuration.GetSection(MEETINGACTIONLINKGERMAN).Value, meetingnotificationLink)
                .Replace(_configuration.GetSection(MEETINGHEADERGERMAN).Value, taskHeaderGerman)
                .Replace(_configuration.GetSection(MEETINGDATAGERMAN).Value, taskData)
                .Replace(_configuration.GetSection(MEETINGLINKINFOGERMAN).Value, meetingLinkInfoGerman)
                .Replace(_configuration.GetSection(CONFIDENTIALGERMAN).Value, isConfidential ? "Vertraulich" : "")
                .Replace(TASKHEADER, GetTaskHeader());

                return returnResult;

            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private string InsertTaskData(string taskTitle, string TaskStatusValue, string responsible, long meetingId, string meetingName,
                    string meetingDate)
        {
            StringBuilder proccessMeetingDate = new StringBuilder();
            if (meetingId > 0)
            {
                proccessMeetingDate.Append("<td style='font-size: 14px; font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; text-align: left; vertical-align: top; padding: 0 0.5em 0 0; ' width='20%'>");
                proccessMeetingDate.Append(taskTitle);
                proccessMeetingDate.Append("</td>");
                proccessMeetingDate.Append("<td style='font-size: 14px; font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; text-align: left; vertical-align: top; padding: 0 0.5em 0 0; ' width='13%'>");
                proccessMeetingDate.Append(TaskStatusValue);
                proccessMeetingDate.Append("</td>");
                proccessMeetingDate.Append("<td style='font-size: 14px; font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; text-align: left; vertical-align: top;' width='27%'>");
                proccessMeetingDate.Append(responsible);
                proccessMeetingDate.Append("</td>");

                proccessMeetingDate.Append("<td style='font-size:14px;font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif;text-align:left; vertical-align: top; padding: 0 0.5em 0 0; ' width='25%'>");
                proccessMeetingDate.Append(meetingName);
                proccessMeetingDate.Append("</td>");
                proccessMeetingDate.Append("<td style='font-size:14px;font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif;text-align:left; vertical-align: top;' width='15%'>");
                proccessMeetingDate.Append(meetingDate);
                proccessMeetingDate.Append("</td>");
            }
            else
            {
                proccessMeetingDate.Append("<td style='font-size: 14px; font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; text-align: left; vertical-align: top; padding: 0 0.5em 0 0; ' width='35%'>");
                proccessMeetingDate.Append(taskTitle);
                proccessMeetingDate.Append("</td>");
                proccessMeetingDate.Append("<td style='font-size: 14px; font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; text-align: left; vertical-align: top; padding: 0 0.5em 0 0; ' width='30%'>");
                proccessMeetingDate.Append(TaskStatusValue);
                proccessMeetingDate.Append("</td>");
                proccessMeetingDate.Append("<td style='font-size: 14px; font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; text-align: left; vertical-align: top;' width='35%'>");
                proccessMeetingDate.Append(responsible);
                proccessMeetingDate.Append("</td>");
            }
            return proccessMeetingDate.ToString();
        }
        private string InsertTaskHeader(long meetingId, bool isGerman)
        {
            StringBuilder proccessMeetingHeader = new StringBuilder();
            proccessMeetingHeader.Append("<th style='font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; text-align: left;'>" + (isGerman ? "Aufgabentitel" : "Task Title") + "</th>");
            proccessMeetingHeader.Append("<th style='font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; text-align: left;'>" + (isGerman ? "Status" : "Status") + "</th>");
            proccessMeetingHeader.Append("<th style='font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; text-align: left;'>" + (isGerman ? "Aktualisiert von" : "Updated By") + "</th>");
            if (meetingId > 0)
            {
                proccessMeetingHeader.Append("<th style='font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif;text-align:left;'>" + (isGerman ? "Meetingbezeichnung" : "Meeting Name") + "</th>");
                proccessMeetingHeader.Append("<th style='font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif;text-align:left;'>" + (isGerman ? "Meeting Datum" : "Meeting Date") + "</th>");
            }
            return proccessMeetingHeader.ToString();
        }

        private string InsertMeetingLink(long mId, string meetingnotificationLink, bool isGerman)
        {
            string meetinglink = string.Concat("<a href='", meetingnotificationLink, "' style='color: #ffffff; text-decoration:none'>").ToString();
            StringBuilder processMeetingLink = new StringBuilder();
            processMeetingLink.Append("<tr>");
            processMeetingLink.Append("<td style='padding: 0 2.1em 0.4em' align='left'>");
            processMeetingLink.Append("<br />");
            processMeetingLink.Append("<span style='font-size: 14px; font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif;'>");
            processMeetingLink.Append(isGerman ? "Für mehr Informationen zu Ihrem Meeting." : "For more information on your Meeting");
            processMeetingLink.Append("</span><br />");
            processMeetingLink.Append("</td>");
            processMeetingLink.Append("</tr>");
            processMeetingLink.Append("<tr style='padding-top:8px'>");
            processMeetingLink.Append("<td bgcolor='#FFFFFF' style='padding: 0 2em;'>");
            processMeetingLink.Append("<table border='0' cellpadding='0' cellspacing='0' width='200px'>");
            processMeetingLink.Append("<tr>");
            processMeetingLink.Append("<td align='center' height='32' style=' padding-right:24px;padding:0px 12px; height:32px; font-size:14px; line-height:12px;width:180px;background-color:#087a94;color:#ffffff;border-radius:18px; font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif;	font-weight:bold;border:none;'>");
            processMeetingLink.Append(meetinglink);
            processMeetingLink.Append(isGerman ? "Gehe zu eLite Meeting" : "Go to eLite Meeting");
            processMeetingLink.Append("</a>");
            processMeetingLink.Append("</td>");
            processMeetingLink.Append("<td bgcolor='#ffffff'>");
            processMeetingLink.Append("</td>");
            processMeetingLink.Append("<td bgcolor='#ffffff'>");
            processMeetingLink.Append("</td>");
            processMeetingLink.Append("</tr>");
            processMeetingLink.Append("</table>");
            processMeetingLink.Append("</td>");
            processMeetingLink.Append("</tr>");
            return processMeetingLink.ToString();
        }
        private string ProccessLink(string meetinglink, string meetingid)
        {

            string proccessLink = string.Empty, proccessId;
            proccessLink = !string.IsNullOrEmpty(meetinglink) ? meetinglink : string.Empty;
            proccessId = !string.IsNullOrEmpty(meetingid) ? meetingid : string.Empty;
            if (!string.IsNullOrEmpty(meetinglink))
            {
                var listMeetingLink = meetinglink.Split("id=;");
                if (listMeetingLink != null && listMeetingLink.Length > 0)
                    proccessLink = listMeetingLink[0] + "id=" + proccessId + ";" + listMeetingLink[1];
            }
            return proccessLink;
        }
        private string GetTaskHeader()
        {
            StringBuilder headerContent = new StringBuilder();

                headerContent.Append("<td style='background: #000; padding: 0px 5em; padding-bottom: 3px; text-align:center; width:100%'>")
                    .Append("<span style='color: #c0c0c0; font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; font-size: 11px;'>")
                    .Append("***This is an automatically generated e-mail by eLite application, please do not reply to this e-mail*** </span></td>");

            return headerContent.ToString();
        }
    }
}
