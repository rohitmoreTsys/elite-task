using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Auth.Token.Lib.Common
{
    public enum AuthRequestSource
    {
        Oidc=0,
        Meeting=1,
        Task=2,
        Topic=3,
        Person=4,
        Admin=5,
        Attachment=6,
        PdfViewer=7
    }
}
