using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.HtmlAgilityPack
{
    public class HtmlAgilityUtility
    {
        public static string ConvertHTMLToString(string html)
        {
            if (!string.IsNullOrWhiteSpace(html))
            {
                var pageContent = System.Web.HttpUtility.HtmlDecode(html);
                var pageDoc = new HtmlDocument();
                pageDoc.LoadHtml(pageContent);
                return pageDoc.DocumentNode.InnerText;
            }
            return null;
        }
    }
}
