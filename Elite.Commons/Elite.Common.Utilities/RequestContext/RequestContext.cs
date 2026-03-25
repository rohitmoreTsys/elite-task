using Elite.Common.Utilities.Encription;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.RequestContext
{
    public sealed class RequestContext : IRequestContext
    {
        private readonly IConfiguration _configuration;

        private readonly IHttpContextAccessor _accessor;
        private const string key = "";
        public RequestContext(IHttpContextAccessor accessor, IConfiguration configuration)
        {
            this._accessor = accessor;
            this._configuration = configuration;
        }

        public IHttpContextAccessor HttpContextAccessor
        {
            get
            {

                return _accessor;
            }
        }

        public bool IsHttpContextExist
        {
            get
            {

                return _accessor != null && _accessor.HttpContext != null;
            }
        }
        public string UID
        {
            get
            {
                return AesCryption.DecryptUID(this._accessor.HttpContext.Request.Headers[_configuration.GetSection("RequestHeaders:UIDRequestHeaderKey").Value], key);
            }
        }

        public string DecrpUID
        {
            get
            {
                if (!string.IsNullOrEmpty(this._accessor.HttpContext.Request.Headers["securedUID"]))
                    return this._accessor.HttpContext.Request.Headers["securedUID"].ToString().Decrypt();
                else
                    return this._accessor.HttpContext.Request.Headers["securedUID"].ToString();
            }
        }
        public string SecuredUID
        {
            get
            {
                return this._accessor.HttpContext.Request.Headers["securedUID"].ToString();
            }
        }

        public string DeputyUID
        {
            get
            {
                return this._accessor.HttpContext.Request.Headers[_configuration.GetSection("RequestHeaders:DeputyUIDRequestHeaderKey").Value];
            }
        }
        public string tokenInfo
        {
            get
            {

                this._accessor.HttpContext.Request.Cookies.TryGetValue("PInfo", out var PInfo);
                return PInfo;
            }
        }

        public bool IsDeputy
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.DeputyUID);
            }
        }

        public DateTime LoginDate
        {
            get
            {

                // return  Convert.ToDateTime(this._accessor.HttpContext.Request.Headers[_configuration.GetSection("RequestHeaders:LoginDate").Value]);
                return DateTime.UtcNow;
            }
        }
        public string SelectedLang
        {
            get
            {
                return this._accessor.HttpContext.Request.Headers["selectedLanguage"];
            }
        }
    }
}
