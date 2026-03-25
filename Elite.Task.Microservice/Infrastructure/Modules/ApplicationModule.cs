using Autofac;
using Elite.Task.Microservice.Application.CQRS.Queries;
using Elite.Task.Microservice.Repository;
using Elite.Task.Microservice.Repository.Contracts;
using Elite_Task.Microservice.Application.CQRS.Queries;
using Elite_Task.Microservice.Core;

using Elite_Task.Microservice.Models;
using Elite_Task.Microservice.Models.Entities;
using Elite_Task.Microservice.Repository;
using Elite_Task.Microservice.Repository.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite_Task.Microservice.Infrastructure.Modules
{
    public class ApplicationModule : Autofac.Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EliteTaskContext>()
          .AsImplementedInterfaces()
          .InstancePerLifetimeScope();

            builder.RegisterType<TaskQueries>()
               .As<ITaskQueries>()
               .InstancePerLifetimeScope();

            builder.RegisterType<TaskCommentQueries>()
             .As<ITaskCommentQueries>()
             .InstancePerLifetimeScope();

            builder.RegisterType<TaskRepository>()
              .As<ITaskRepository>()
              .InstancePerLifetimeScope();

            builder.RegisterType<TaskAttachmentRepository>()
              .As<ITaskAttachmentRepository>()
              .InstancePerLifetimeScope();

            builder.RegisterType<RepositoryEventStore>()
             .As<IRepositoryEventStore>()
             .InstancePerDependency();

            builder.RegisterType<CommentRepository>()
            .As<ICommentRepository>()
            .InstancePerDependency();

        }
    }
}
