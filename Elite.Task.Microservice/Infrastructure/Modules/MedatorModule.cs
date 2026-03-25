using Autofac;
using Elite.Task.Microservice.Application.CQRS.Commands;
using Elite_Task.Microservice.Application.Behaviors;
using Elite_Task.Microservice.Application.CQRS.Commands;
using FluentValidation;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Elite_Task.Microservice.Infrastructure.Modules
{
    public class MedatorModule
        : Autofac.Module
    {

        protected override void Load(ContainerBuilder builder)
        {

            builder.RegisterAssemblyTypes(typeof(IMediator).GetTypeInfo().Assembly)
             .AsImplementedInterfaces();


            builder.RegisterAssemblyTypes(typeof(TaskCommand).GetTypeInfo().Assembly)
                .AsClosedTypesOf(typeof(IRequestHandler<,>));

            builder.RegisterAssemblyTypes(typeof(JiraTaskCommand).GetTypeInfo().Assembly)
               .AsClosedTypesOf(typeof(IRequestHandler<,>));


            builder.RegisterAssemblyTypes(typeof(TaskCommand).GetTypeInfo().Assembly)
                        .Where(t => t.IsClosedTypeOf(typeof(IValidator<>)))
                        .AsImplementedInterfaces();

            builder.RegisterAssemblyTypes(typeof(JiraTaskCommand).GetTypeInfo().Assembly)
                        .Where(t => t.IsClosedTypeOf(typeof(IValidator<>)))
                        .AsImplementedInterfaces();

            builder.Register<SingleInstanceFactory>(context =>
            {
                var componentContext = context.Resolve<IComponentContext>();
                return t => { object o; return componentContext.TryResolve(t, out o) ? o : null; };
            });

            builder.Register<MultiInstanceFactory>(context =>
            {
                var componentContext = context.Resolve<IComponentContext>();
                return t =>
                {
                    var resolved = (IEnumerable<object>)componentContext.Resolve(typeof(IEnumerable<>).MakeGenericType(t));
                    return resolved;
                };
            });

            builder.RegisterGeneric(typeof(ValidatorBehavior<,>)).As(typeof(IPipelineBehavior<,>));

        }
    }
}
