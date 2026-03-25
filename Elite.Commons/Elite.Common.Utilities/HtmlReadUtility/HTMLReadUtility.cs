using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.HtmlReadUtility
{
    public class HTMLReadUtility : IHTMLReadUtility
    {

        private string _fileContent;

        protected readonly IConfiguration _configuration;
        private IHostingEnvironment _env;

        public string FileContent => string.Copy(_fileContent);


        public HTMLReadUtility(IHostingEnvironment env, string fileName)
        {
            string fileContent;


            _env = env;
            var webRoot = _env.ContentRootPath;

            fileContent = System.IO.Path.Combine(webRoot, fileName);


            if (System.IO.File.Exists(fileContent))
                _fileContent = System.IO.File.ReadAllText(fileContent);
            else
                _fileContent = string.Empty;


        }
    }
}
