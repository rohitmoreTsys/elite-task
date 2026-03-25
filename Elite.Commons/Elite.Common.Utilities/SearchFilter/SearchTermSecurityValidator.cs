using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Elite.Common.Utilities.CommonType;
using Elite.Common.Utilities.SecurityFilters;
using Microsoft.AspNetCore.Mvc;

public class SearchTermSecurityValidator
{



    //private static readonly string[] _securityPatterns = new[]
    //{
    //    // SQL Injection Patterns
    //    "select * from", "drop table", "insert into", "delete from",
    //    "union select", "union all select",
    //    "exec(", "execute(", "sp_", "xp_", "alter table", "create table",
    //    "information_schema", "sysobjects", "syscolumns", "master..", "msdb..",
    //    "waitfor delay", "benchmark(", "pg_sleep(", "sleep(",
    //    "' or '1'='1", "' or 1=1", "\" or \"1\"=\"1", "\" or 1=1",
    //    "'; drop", "\"; drop", "' union", "\" union", "' having", "\" having",

    //    // XSS Patterns
    //    "<script", "onerror", "onload", "javascript:", "vbscript:", "data:text/html",
    //    "<iframe", "src=\"javascript:", "src='javascript:", "src=javascript:",
    //    "<object", "<embed", "<applet", "<form", "<input", "<textarea", "<select",
    //    "<button", "<link", "<meta", "<base", "<style", "expression(",
    //    "url(javascript:", "url('javascript:", "url(\"javascript:",
    //    "&#x", "&#X", "&amp;", "&quot;", "&#39;", "&#34;", "&#60;", "&#62;",

    //    // Path Traversal Patterns
    //    "../", "..\\", "%2e%2e", "%2e%2e%2f", "%2e%2e%5c", "%2e%2e/", "%2e%2e\\",
    //    "..%2f", "..%5c", "..%c0%af", "..%c1%9c", "%c0%ae%c0%ae%c0%af",
    //    "....//", "....\\\\", ".%2e/", ".%2e\\", "%252e%252e",
    //    "/etc/passwd", "/etc/shadow", "/proc/", "/sys/", "c:\\windows\\",
    //    "c:/windows/", "boot.ini", "win.ini", "system.ini",

    //    // Command Injection Patterns
    //    "cmd.exe", "powershell", "bash", "/bin/", "sh -c", "cmd /c",
    //    "system(", "exec(", "eval(", "passthru(", "shell_exec(",
    //    "proc_open(", "popen(", "file_get_contents(", "readfile(",
    //    "include(", "require(", "include_once(", "require_once(",
    //    "|", "&&", "||", ";", "`", "$(", "${", "<!--", "-->",

    //    // File Upload/Path Patterns
    //    ".php", ".asp", ".aspx", ".jsp", ".jspx", ".cfm", ".cgi", ".pl", ".py",
    //    ".rb", ".exe", ".bat", ".cmd", ".com", ".scr", ".vbs", ".js",
    //    "web.config", ".htaccess", ".htpasswd", "httpd.conf", "apache2.conf",

    //    // Server-Side Include (SSI) Patterns
    //    "<!--#", "<!--#exec", "<!--#include", "<!--#echo", "<!--#config",
    //    "<!--#set", "<!--#if", "<!--#elif", "<!--#else", "<!--#endif",

    //    // LDAP Injection Patterns
    //    "*(", "*)", "!(", "!)", "&(", "&)", "|(", "|)", "=(", "=)",
    //    "~=(", "~=)", ">=(", ">=)", "<=(", "<=)", "cn=", "ou=", "dc=",

    //    // NoSQL Injection Patterns
    //    "$where", "$ne", "$in", "$nin", "$gt", "$lt", "$gte", "$lte",
    //    "$regex", "$options", "$mod", "$all", "$size", "$exists", "$type",
    //    "$or", "$and", "$not", "$nor", "this.", "function(", "return ",

    //    // URL Encoded Variations
    //    "%3cscript", "%2527", "%3c", "%3e", "&lt;", "&gt;", "&#x27;",
    //    "%22", "%27", "%3d", "%26", "%7c", "%3b", "%28", "%29",
    //    "%20or%20", "%20and%20", "%20union%20", "%20select%20",
    //    "%3cimg", "%3ciframe", "%3cobject", "%3cembed", "%3clink",

    //    // Double Encoding
    //    "%253c", "%2522", "%2527", "%253e", "%252f", "%255c",
    //    "%2520or%2520", "%2520and%2520", "%2520union%2520",

    //    // Unicode Encoding
    //    "\\u003c", "\\u0022", "\\u0027", "\\u003e", "\\u002f", "\\u005c",
    //    "\\u0020or\\u0020", "\\u0020and\\u0020", "\\u0020union\\u0020",

    //    // XML External Entity (XXE) Patterns
    //    "<!entity", "<!doctype", "system ", "public ", "<!\\[cdata\\[",
    //    "&xxe;", "file://", "http://", "https://", "ftp://", "gopher://",

    //    // Server-Side Request Forgery (SSRF) Patterns
    //    "localhost", "127.0.0.1", "0.0.0.0", "::1", "169.254.169.254",
    //    "192.168.", "10.", "172.16.", "172.17.", "172.18.", "172.19.",
    //    "172.20.", "172.21.", "172.22.", "172.23.", "172.24.", "172.25.",
    //    "172.26.", "172.27.", "172.28.", "172.29.", "172.30.", "172.31.",

    //    // Additional Dangerous Functions/Keywords
    //    "document.cookie", "document.write", "innerHTML", "outerHTML",
    //    "createelement", "appendchild", "insertbefore", "settimeout",
    //    "setinterval", "window.open", "location.href", "location.replace",
    //    "fromcharcode", "string.fromcharcode", "unescape", "decodeuri",
    //    "decodeuricomponent", "atob", "btoa", "activexobject",

    //    // Template Injection Patterns
    //    "{{", "}}", "${", "#{", "<%", "%>", "<#", "#>", "[[", "]]",
    //    "{{7*7}}", "${7*7}", "#{7*7}", "<%=7*7%>", "<#assign",

    //    // Header Injection Patterns
    //    "\\r\\n", "\\n", "\\r", "%0d%0a", "%0a", "%0d", "content-type:",
    //    "set-cookie:", "location:", "refresh:", "x-forwarded-for:",

    //    // Additional Encoding Variants
    //    "\\x3c", "\\x22", "\\x27", "\\x3e", "\\x2f", "\\x5c", "\\x20",
    //    "\\074", "\\042", "\\047", "\\076", "\\057", "\\134", "\\040"
    //};
    //private static readonly string[] _securityPatterns = new[]
    //{
    //    "select * from", "drop table", "insert into", "delete from", "update ", "truncate ",
    //    "<script", "onerror", "onload", "javascript:",
    //    "../", "..\\", "%2e%2e", "%3cscript", "%2527", "%3c", "%3e", "&lt;", "&gt;", "&#x27;"
    //};

    //private static readonly List<SecurityPattern> _securityPatterns = new List<SecurityPattern>
    //{
    //    // SQL Injection
    //    new SecurityPattern(@"(--|#)", "SQL comment injection", ThreatType.SqlInjection),
    //    new SecurityPattern(@"(\bOR\b|\bAND\b)\s+\d+\s*=\s*\d+", "SQL boolean injection", ThreatType.SqlInjection),
    //    new SecurityPattern(@"(\bUNION\b\s+\bSELECT\b)", "SQL UNION injection", ThreatType.SqlInjection),
    //    //new SecurityPattern(@"(\bINSERT\b|\bDELETE\b|\bUPDATE\b|\bDROP\b)\s+", "SQL modification command", ThreatType.SqlInjection),
    //    // SQL Injection - SELECT patterns
    //new SecurityPattern(@"\bSELECT\b\s+\*\s+\bFROM\b", "SELECT * FROM injection", ThreatType.SqlInjection),
    //new SecurityPattern(@"\bSELECT\b\s+.*\bFROM\b\s+\w+", "SELECT FROM injection", ThreatType.SqlInjection),

    //    // XSS - More specific patterns
    //    new SecurityPattern(@"<script[^>]*>.*?</script>", "Script tag injection", ThreatType.XssAttack),
    //    new SecurityPattern(@"on\w+\s*=\s*[""'][^""']*[""']", "HTML event handler injection", ThreatType.XssAttack),
    //    new SecurityPattern(@"javascript:\s*\w+", "JavaScript protocol injection", ThreatType.XssAttack),
    //    new SecurityPattern(@"<iframe[^>]*>", "Iframe injection", ThreatType.XssAttack),

    //    // Path Traversal
    //    new SecurityPattern(@"(\.\./|\.\.\\){2,}", "Directory traversal attempt", ThreatType.PathTraversal),
    //    new SecurityPattern(@"/(etc|boot|windows|system32)/", "System path access", ThreatType.PathTraversal),

    //    // Meeting Search Specific - be more lenient with quotes
    //    //new SecurityPattern(@"['""];?\s*(DROP|DELETE|UPDATE)\s+", "Dangerous SQL with quotes", ThreatType.SqlInjection)
    //};

    //public static ValidationResult ValidateSearchTerm(string encodedSearchTerm)
    //{
    //    try
    //    {
    //        var searchBytes = Convert.FromBase64String(encodedSearchTerm);

    //        var decodedSearchTerm = SanitizeInput(Encoding.UTF8.GetString(searchBytes));

    //        var normalizedTerm = decodedSearchTerm.Trim();

    //        foreach (var securityPattern in _securityPatterns)
    //        {
    //            if (_securityPatterns.Any(pattern => normalizedTerm.Contains(pattern)))
    //                return ValidationResult.CreateFailed(securityPattern);
    //            //if (Regex.IsMatch(normalizedTerm, securityPattern.Pattern,
    //            //    RegexOptions.IgnoreCase | RegexOptions.Multiline))
    //            //{
    //            //    return ValidationResult.CreateFailed(securityPattern);
    //            //}
    //        }

    //        return ValidationResult.CreateSuccess(normalizedTerm);
    //    }
    //    catch (FormatException)
    //    {
    //        return ValidationResult.CreateFailed("Invalid search format");
    //    }
    //    catch (Exception)
    //    {
    //        return ValidationResult.CreateFailed("Validation error occurred");
    //    }
    //}

    //public static string SanitizeInput(string input)
    //{
    //    if (string.IsNullOrWhiteSpace(input)) return string.Empty;

    //    string noHtml = Regex.Replace(input, "<.*?>", string.Empty);

    //    return Regex.Replace(noHtml, @"[<>""'%;]", string.Empty);
    //}
    private static readonly string[] _securityPatterns = new[]
    {
        @"\bselect\s+\*\s+from\b",
        @"\bdrop\s+table\b",
        @"\binsert\s+into\b",
        @"\bdelete\s+from\b",
        @"\bunion(\s+all)?\s+select\b",
        @"\b(waitfor\s+delay|benchmark\(|pg_sleep\(|sleep\()\b",
        @"\b(sp_|xp_)[A-Za-z0-9_]*\b",
        @"\binformation_schema\b",
        @"('|"" )\s*or\s*\1?1\1?\s*=\s*\1?1\1?",
        @"<\s*script\b",
        @"\bonerror\s*=",
        @"\bjavascript\s*:",
        @"\bsrc\s*=\s*(['""]?)\s*javascript\s*:",
        @"{{|}}",
        @"\bng-(click|mouseover|focus|blur|change|include|bind-html|src|href)\b",
        @"(?:\.\.\/|\.\.\\|%2e%2e|%252e%252e)",
        @"\b(?:\/etc\/passwd|\/etc\/shadow|boot\.ini|win\.ini|system\.ini)\b",
        @"(?:\||&&|;;|;|`|\$\(|\$\{)",
        @"\b(?:system|exec|eval|passthru|shell_exec|proc_open|popen)\s*\(",
        @"\b\.[pP][hH][pP]\b",
        @"\b\.[jJ][sS]\b",
        @"<!--\s*#(include|exec|echo|config)\b",
        @"\b(?:cn=|ou=|dc=)",
        @"\$(?:where|ne|in|nin|gt|lt|gte|lte|regex|options|or|and|not|nor)\b",
        @"<!\s*(?:doctype|ENTITY)",
        @"\b(?:file|https?|ftp):\/\/[^\s'""]+",
        @"\b(?:localhost|127\.0\.0\.1|::1)\b",
        @"\b169\.254\.169\.254\b",
        @"\b10\.(?:\d{1,3}\.){2}\d{1,3}\b",
        @"\b172\.(?:1[6-9]|2\d|3[01])\.\d{1,3}\.\d{1,3}\b",
        @"\b192\.168\.\d{1,3}\.\d{1,3}\b",
        @"\bdocument\.cookie\b",
        @"\binnerHTML\b",
        @"(\$\{[^}]+\}|<%|%>)",
        @"(?:\r\n|\n|\r|%0d%0a|%0a|%0d)"
    };
    public static ValidationResult ValidateSearchTerm(string encodedSearchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(encodedSearchTerm))
                return ValidationResult.CreateFailed("Empty search term");

            byte[] searchBytes;
            try
            {
                searchBytes = Convert.FromBase64String(encodedSearchTerm);
            }
            catch
            {
                return ValidationResult.CreateFailed("Invalid Base64 format");
            }

            var decodedSearchTerm = Encoding.UTF8.GetString(searchBytes);
            var sanitizedTerm = SanitizeInput(decodedSearchTerm).Trim();

            foreach (var pattern in _securityPatterns)
            {
                if (Regex.IsMatch(sanitizedTerm, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline))
                {
                    return ValidationResult.CreateFailed($"Security threat detected ({pattern})");
                }
            }

            return ValidationResult.CreateSuccess(sanitizedTerm);
        }
        catch (Exception ex)
        {
            return ValidationResult.CreateFailed($"Validation error: {ex.Message}");
        }
    }

    public static string SanitizeInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        string noHtml = Regex.Replace(input, "<.*?>", string.Empty);

        return Regex.Replace(noHtml, @"[<>""'%;]", string.Empty);
    }
}