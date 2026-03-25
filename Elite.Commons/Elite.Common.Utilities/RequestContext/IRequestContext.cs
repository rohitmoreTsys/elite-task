using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.RequestContext
{
    public interface IRequestContext
    {
        string UID { get; }
        string DeputyUID { get;  }
        string tokenInfo { get; }
        bool  IsHttpContextExist { get; }
        bool IsDeputy { get; }
        DateTime LoginDate { get; }
        IHttpContextAccessor HttpContextAccessor { get; }
        string SelectedLang { get; }
        string DecrpUID { get; }
        string SecuredUID { get; }

    }
}
