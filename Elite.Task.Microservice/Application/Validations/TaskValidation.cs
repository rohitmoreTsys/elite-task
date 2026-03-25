using Elite_Task.Microservice.Application.CQRS.Commands;
using Elite_Task.Microservice.Application.CQRS.Commands.CommmandsDto;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite_Task.Microservice.Application.Validations
{
    public class TaskValidation : AbstractValidator<TaskCommand>
    {
        protected readonly IConfiguration _configuration;

        public TaskValidation(IConfiguration configuration)
        {
            this._configuration = configuration;


            RuleFor(task => task.Title).NotEmpty().WithMessage("Title is Required")
               .DependentRules(() =>
               {
                   RuleFor(d2 => d2.Title).MaximumLength(200);
               }).WithMessage((p) => { return $"({p.Title}) length should be 200 character"; });

            //Topic Description
            //RuleFor(task => task.Description).MaximumLength(2000).When(p => !string.IsNullOrWhiteSpace(p.Description)).WithMessage((p) =>
            //{
            //    return $"({p.Description}) length should be 2000 character";
            //});

            //Responsible
            RuleFor(task => task.Responsible).NotNull().WithMessage("Responsible is Required");

            //DueDate
            RuleFor(task => task.DueDate).NotNull().WithMessage("DueDate is Required");

            //FileLink
            RuleFor(task => task.FileLink).MaximumLength(200).When(p => !string.IsNullOrWhiteSpace(p.FileLink)).WithMessage((p) =>
            {
                return $"({p.FileLink}) length should be 200 character";
            });

            //Sub task
            RuleFor(task => task.SubTask).SetValidator(new RegisterTakEnumerableValidator(this._configuration));

            //Attachments
            RuleFor(task => task.Attachments).SetValidator(new FileSizeValidation(this._configuration));

        }
    }


    public class FileSizeValidation : AbstractValidator<IList<TaskAttachmentCommandDto>>
    {
        public FileSizeValidation(IConfiguration configuration)
        {
            RuleFor(file => file.Where(x => x.IsDeleted == false).Sum(p => p.AttachmentSize)).LessThanOrEqualTo(Convert.ToInt64(configuration.GetSection("MaxFileSize").Value)).WithMessage((p) => { return $"Attached Files exceeds " + configuration.GetSection("MaxFileSizeInMB").Value; });
        }

    }

    public class RegisterTakEnumerableValidator : AbstractValidator<IList<TaskCommand>>
    {
        public RegisterTakEnumerableValidator(IConfiguration configuration) =>
            RuleForEach(model => model).SetValidator(new SubTaskValidation(configuration));
    }

    public class SubTaskValidation : AbstractValidator<TaskCommand>
    {
        public SubTaskValidation(IConfiguration configuration)
        {
            //Title
            RuleFor(task => task.Title).NotEmpty().WithMessage("Sub Task Title is Required")
               .DependentRules(() =>
               {
                   RuleFor(d2 => d2.Title).MaximumLength(200);
               }).WithMessage((p) => { return $"Sub Task ({p.Title}) length should be 200 character"; });

            ////Topic Description
            //RuleFor(task => task.Description).MaximumLength(2000).When(p => !string.IsNullOrWhiteSpace(p.Description)).WithMessage((p) =>
            //{
            //    return $"Sub Task ({p.Description}) length should be 2000 character";
            //});

            //Responsible
            RuleFor(task => task.Responsible).NotNull().WithMessage("Sub Task Responsible is Required");

            //DueDate
            RuleFor(task => task.DueDate).NotNull().WithMessage("Sub Task DueDate is Required");
        }

    }
}
