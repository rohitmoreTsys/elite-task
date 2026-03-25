using Elite.Common.Utilities.RequestContext;
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

namespace Elite.Common.Utilities.FileUpload
{
    public class MultiPartFormDataRequestHelper<T> where T : IAttachFiles, ICloneable, new()
    {

        public static async Task<T> GetTopicCommandAsync(HttpContext context, HttpRequest request)
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
                        targetFilePath = Path.GetTempFileName();
                        using (var targetStream = System.IO.File.Create(targetFilePath))
                        {
                            await section.Body.CopyToAsync(targetStream);




                            AttachFile file = new AttachFile();
                            file.FilePath = targetFilePath;
                            try
                            {
                                file.FileName = JsonConvert.DeserializeObject<string>(contentDisposition.FileName);
                            }
                            catch
                            {
                                file.FileName = contentDisposition.FileName;
                            }

                            file.FileSize = targetStream.Length;
                            command.Files.Add(file);
                        }
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
                                throw new NullReferenceException($"Expected { nameof(T) } value came as null while processing Multi Part Form Data Request");

                            var jsonData = JsonConvert.DeserializeObject<T>(value);
                            T data = (T)jsonData.Clone();
                            if (command.Files?.Count > 0)
                                data.Files = command.Files;
                            command = data;


                        }
                    }
                }
                section = await reader.ReadNextSectionAsync();
            }

            return command;
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
