using Elite.Common.Utilities.FileUpload;
using Elite_Task.Microservice.Application.CQRS.Commands;
using Elite_Task.Microservice.CommonLib;

using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite_Task.Microservice.Application.Validations
{
    public class TopicAttachmentValidation : AbstractValidator<AttachFile>
    {   
        public TopicAttachmentValidation(IConfiguration configuration) 
        {
             RuleFor(file => file.FileSize).GreaterThan(0).WithMessage((p) => { return $"File ({p.FileName}) is empty"; });

                RuleFor(file => file.FileSize).LessThanOrEqualTo(Convert.ToInt64(configuration.GetSection("MaxFileSize").Value)).WithMessage((p) => { return $"File ({p.FileName}) exceeds 40 MB."; });    
        }
    }
}
