using Elite.AntiVirus.Lib;
using Elite.AntiVirus.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Elite.Common.Utilities.FileUpload
{
    public class MultiPartFormDataRequestAVHelper<T> where T : IAttachAVFiles, ICloneable, new()
    {
        public static async Task<T> GetTopicCommandAsync(HttpContext context, HttpRequest request, IConfiguration config)
        {
            FormOptions _defaultFormOptions = new FormOptions();
            string targetFilePath = null;
            var command = new T();

            if (!MultipartRequestHelper.IsMultipartContentType(request.ContentType))
            {
                throw new InvalidDataException("Expected a multipart request, but got {Request.ContentType}");
            }

            var boundary = MultipartRequestHelper.GetBoundary(
               MediaTypeHeaderValue.Parse(request.ContentType),
               _defaultFormOptions.MultipartBoundaryLengthLimit);
            var reader = new Microsoft.AspNetCore.WebUtilities.MultipartReader(boundary, context.Request.Body);
            var section = await reader.ReadNextSectionAsync();

            while (section != null)
            {
                if (section.ContentDisposition == null)
                    break;
                System.Net.Http.Headers.ContentDispositionHeaderValue contentDisposition;
                var hasContentDispositionHeader = System.Net.Http.Headers.ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);

                if (hasContentDispositionHeader)
                {
                    if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                    {
                        var filePath = config.GetSection("ConnectionConfiguration:fileTempPath").Value;

                        targetFilePath = filePath + Guid.NewGuid().ToString() + ".tmp";

                        //EPA issue fix
                        if (IsDirectoryTraversal(contentDisposition.FileName))
                            continue;


                        string hostname = config.GetSection("clamAV:hostname").Value;
                        int port = Int32.Parse(config.GetSection("clamAV:port").Value);
                        long maxStreamSize = long.Parse(config.GetSection("clamAV:maxStreamSize").Value);
                        //made it configurable to test it on local when clamav is not set-up
                        var isVirusScanEnabled = bool.Parse(config.GetSection("clamAV:IsVirusScanEnabled").Value);
                        byte[] byteArray;
                        MemoryStream ms = new MemoryStream();
                        section.Body.CopyTo(ms);
                        byteArray = ms.ToArray();
                        ClamAV antivirusCheck = new ClamAV();
                        ScanResult result = isVirusScanEnabled ? await antivirusCheck.ScanFileForViruses(byteArray, hostname, port, maxStreamSize) : new ScanResult() { value = false, message = string.Empty };

                        AttachFileAV file = new AttachFileAV();
                        try
                        {
                            file.FileName = JsonConvert.DeserializeObject<string>(contentDisposition.FileName);
                        }
                        catch
                        {
                            file.FileName = contentDisposition.FileName;
                        }
                        if (result.value == true)
                        {
                            //change virus detected flag to true
                            file.VirusDetected = true;
                        }
                        else
                        {
                            file.VirusDetected = false;
                            //Save Attachment in temp path only when virus not detected
                            using (var targetStream = File.Create(targetFilePath))
                            {
                                ms.Position = 0;
                                await ms.CopyToAsync(targetStream);

                                file.FilePath = targetFilePath;

                                file.FileSize = targetStream.Length;
                            }
                        }

                        //Get the extension of the file being uploaded and check if it's valid.
                        var fileNames = file.FileName.Split('.');

                        //code changed for Loggin purpose

                        //EPA issue fix
                        if (IsAllowedFileExtension(fileNames[fileNames.Length - 1]))
                            file.IsFileExtensionAllowed = true;
                        else
                            file.IsFileExtensionAllowed = false;

                        command.Files.Add(file);

                    }
                    else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                    {
                        var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name);
                        var encoding = GetEncoding(section);
                        using (var streamReader = new StreamReader(
                            section.Body,
                            encoding,
                            detectEncodingFromByteOrderMarks: true,
                            bufferSize: 1024,
                            leaveOpen: true))
                        {
                            var value = await streamReader.ReadToEndAsync();

                            if (value == null)
                                throw new NullReferenceException($"Expected {nameof(T)} value came as null while processing Multi Part Form Data Request");

                            var jsonData = JsonConvert.DeserializeObject<T>(value);
                            try
                            {
                                T data = (T)jsonData.Clone();
                                if (command.Files?.Count > 0)
                                    data.Files = command.Files;
                                command = data;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                }
                section = await reader.ReadNextSectionAsync();
            }

            return command;
        }

        /// <summary>
        /// I.e. they should be requesting 'filename.txt'
        /// but they request '../location/filename.pdf
        /// </summary>
        /// <param name="fileName">The file name to check</param>
        /// <returns></returns>
        private static bool IsDirectoryTraversal(string fileName)
        {
            bool isTraversing = false;

            if (String.IsNullOrWhiteSpace(fileName))
            {
                return isTraversing;
            }

            var decodedFileName = HttpUtility.UrlDecode(fileName);
            if (decodedFileName.Contains("/") ||
                decodedFileName.Contains(@"\") ||
                decodedFileName.Contains("$") ||
                decodedFileName.Contains("..") ||
                decodedFileName.Contains("?"))
            {
                isTraversing = true;
            }

            return isTraversing;
        }
        private static bool IsAllowedFileExtension(string filename)
        {
            var fileExtensions = new List<string>() {  "pdf", "ppt", "pptx", "jpeg", "jpg", "doc", "docx", "txt", "png", "xls", "xlsx", "zip" ,"mp4","mkv","mov","xhtml"};

            return fileExtensions.Contains(filename.ToLower());
        }


        private static Encoding GetEncoding(MultipartSection section)
        {
            MediaTypeHeaderValue mediaType;
            var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);
            // UTF-7 is insecure and should not be honored. UTF-8 will succeed in 
            // most cases.
            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
            {
                return Encoding.UTF8;
            }
            return mediaType.Encoding;
        }
    }
}
