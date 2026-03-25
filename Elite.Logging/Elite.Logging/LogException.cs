using Elite.Common.Utilities.SecretVault;
using Elite.Logging.Models;
using Elite.SmtpEmail;
using Microsoft.Extensions.Configuration;
using System;

namespace Elite.Logging
{
    public class LogExcepion : ILogException
    {
        private readonly EliteLoggerContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _smtpPort;
        private readonly string _smtpAddress;
        private readonly string _eliteOutlookUsername;
        private readonly string _eliteOutlookPassword;
        private readonly SecretVault secretVault;
        public LogExcepion(EliteLoggerContext context, IConfiguration configuration)
        {
            this._configuration = configuration;
            this._context = context;
            secretVault = SecretVault.Instance;
            this._smtpPort = secretVault.GetValuesFromVault("eliteSmtpPort");
            this._smtpAddress = secretVault.GetValuesFromVault("eliteSmtpAddress");
            this._eliteOutlookUsername = secretVault.GetValuesFromVault("eliteOutlookUsername");
            this._eliteOutlookPassword = secretVault.GetValuesFromVault("eliteOutlookPassword");
        }
        private static readonly log4net.ILog log = log4net.LogManager
        .GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public void LogEliteError(string logUserId, Exception e)
        {
            string serviceName = _configuration.GetSection("Environment:serviceName").Value;
            bool isDbEnabled = Convert.ToBoolean(_configuration.GetSection("ErrorFlags:isDbEnabled").Value);
            bool isEmailEnabled = Convert.ToBoolean(_configuration.GetSection("ErrorFlags:isEmailEnabled").Value);
            bool isTextEnabled = Convert.ToBoolean(_configuration.GetSection("ErrorFlags:isTextEnabled").Value);
            string SmtpAddress = this._smtpAddress;
            string SmtpPort = this._smtpPort;
            string SmtpSenderAddress = _configuration.GetSection("Elite_SMTP_EMAIL:SmtpSenderAddress").Value;
            string Environment = _configuration.GetSection("Environment:Name").Value;
            string SmtpReceiverAddressess = _configuration.GetSection("Elite_SMTP_EMAIL:SmtpReceiverAddressess").Value;
            string emailSubject = _configuration.GetSection("Elite_SMTP_EMAIL:Subject").Value;
            string errMessage = e?.Message + e?.InnerException?.ToString() + e?.StackTrace?.ToString();
            string outlookUserName = this._eliteOutlookUsername;
            string outlookPassword = this._eliteOutlookPassword;
            string outlookDomain = _configuration.GetSection("Elite_SMTP_EMAIL:ELiteOutlookDomain").Value;

            try
            {
                if (isTextEnabled)
                {

                    //storing the error in log file using log4net
                    log.Error("Error type :" + e?.GetType());
                    log.Error("Error Description :" + errMessage);
                }
            }
            catch (Exception)
            {
                //Can be ignored. As this catch part is for the Logging.
            }

            try
            {
                if (isDbEnabled)
                {
                    //storing the error in db
                    DateTime dateTime = DateTime.Now.ToUniversalTime();
                    Logs logError = new Logs();
                    logError.LogUserId = logUserId;
                    logError.LogDescription = errMessage;
                    logError.LogDateTime = dateTime;
                    logError.LogServiceName = serviceName;
                    _context.Logs.Add(logError);
                    _context.SaveChanges();
                }
            }
            catch (Exception)
            {
                //Can be ignored. As this catch part is for the Logging.
            }
            try
            {
                if (isEmailEnabled)
                {
                    SmtpSendEmail sendEmailToSupport = new SmtpSendEmail(errMessage, emailSubject + Environment,
                    SmtpAddress,
                    SmtpPort,
                    SmtpSenderAddress,
                    SmtpReceiverAddressess,
                    outlookUserName,
                    outlookPassword,
                    outlookDomain
                    );
                    var sendEmail = sendEmailToSupport.SendEmail();
                }
            }
            catch (Exception)
            {
                //Can be ignored. As this catch part is for the Logging.
            }
        }

        public void LogEliteInfo(string logUserId, Exception e, string ActionType)
        {
            string serviceName = string.Empty;
            if (ActionType.Contains("revoke"))
            {
                serviceName = _configuration.GetSection("Environment:ServiceNameInfoRevoke").Value;
            }
            else if (ActionType.Contains("close"))
            {
                serviceName = _configuration.GetSection("Environment:ServiceNameInfoClose").Value;
            }

            bool isDbEnabled = Convert.ToBoolean(_configuration.GetSection("ErrorFlags:IsDBLoggingEnabled").Value);
            bool isEmailEnabled = Convert.ToBoolean(_configuration.GetSection("ErrorFlags:IsEmailLoggingEnabled").Value);
            bool isTextEnabled = Convert.ToBoolean(_configuration.GetSection("ErrorFlags:IsTextLoggingEnabled").Value);
            string SmtpAddress = this._smtpAddress;
            string SmtpPort = this._smtpPort;
            string emailSubject = _configuration.GetSection("Elite_SMTP_EMAIL:Subject").Value;
            string SmtpSenderAddress = _configuration.GetSection("Elite_SMTP_EMAIL:SmtpSenderAddress").Value;
            string Environment = _configuration.GetSection("Environment:Name").Value;
            string SmtpReceiverAddressess = _configuration.GetSection("Elite_SMTP_EMAIL:SmtpReceiverAddressess").Value;
            string outlookUserName = this._eliteOutlookUsername;
            string outlookPassword = this._eliteOutlookPassword;
            string outlookDomain = _configuration.GetSection("Elite_SMTP_EMAIL:ELiteOutlookDomain").Value;

            string errMessage = e?.Message + e?.InnerException?.ToString() + e?.StackTrace?.ToString();
            try
            {
                if (isTextEnabled)
                {
                    //storing the error in log file using log4net
                    log.Error("Error type :" + e?.GetType());
                    log.Error("Error Description :" + errMessage);
                }
            }
            catch (Exception)
            {
                //Can be ignored. As this catch part is for the Logging.
            }

            try
            {
                if (isDbEnabled)
                {
                    //storing the error in db
                    DateTime dateTime = DateTime.Now.ToUniversalTime();
                    Logs logError = new Logs();
                    logError.LogUserId = logUserId;
                    logError.LogDescription = errMessage;
                    logError.LogDateTime = dateTime;
                    logError.LogServiceName = serviceName;
                    _context.Logs.Add(logError);
                    _context.SaveChanges();
                }
            }
            catch (Exception)
            {
                //Can be ignored. As this catch part is for the Logging.
            }
            try
            {
                if (isEmailEnabled)
                {
                    SmtpSendEmail sendEmailToSupport = new SmtpSendEmail(errMessage, emailSubject + Environment,
                    SmtpAddress,
                    SmtpPort,
                    SmtpSenderAddress,
                    SmtpReceiverAddressess,
                     outlookUserName,
                    outlookPassword,
                    outlookDomain);
                    var sendEmail = sendEmailToSupport.SendEmail();
                }
            }
            catch (Exception)
            {
                //Can be ignored. As this catch part is for the Logging.
            }
        }
    }
}
