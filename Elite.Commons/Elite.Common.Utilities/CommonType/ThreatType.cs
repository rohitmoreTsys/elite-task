using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.CommonType
{
    public enum ThreatType
    {
        SqlInjection,
        XssAttack,
        PathTraversal,
        GeneralInjection,
        CommandInjection,
        LdapInjection,
        XmlInjection,
        HeaderInjection,
        NoSqlInjection,
        FileUpload,
        AuthBypass,
    }
}