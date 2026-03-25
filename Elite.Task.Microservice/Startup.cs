using Autofac;
using Autofac.Extensions.DependencyInjection;
using DinkToPdf;
using DinkToPdf.Contracts;
using Elite.Auth.Token.Lib.Services;
using Elite.Common.Utilities.Attachment.Delete.MapOrphans;
using Elite.Common.Utilities.CommonType;
using Elite.Common.Utilities.ExceptionHandling.ExceptionFilters;
using Elite.Common.Utilities.RequestContext;
using Elite.Common.Utilities.SecretVault;
using Elite.EventBus.Services;
using Elite.Filters.Lib.Services;
using Elite.Logging;
using Elite.Logging.Models;
using Elite.OIDC.Handler.Lib.Auth;
using Elite.OIDC.Handler.Lib.AuthFilters;
using Elite.OIDC.Handler.Lib.Model;
using Elite.Task.Microservice.Application.CQRS.ExternalService;
using Elite.Task.Microservice.Infrastructure.HostedServices;
using Elite.Task.Microservice.NotificationServices;
using Elite_Task.Microservice.Application.CQRS.ExternalService;
using Elite_Task.Microservice.Infrastructure.Modules;
using Elite_Task.Microservice.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Data.Common;

namespace Elite_Task.Microservice
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var secretVault = SecretVault.Instance;

            services.AddMvc();
            services.AddEntityFrameworkNpgsql()
              .AddDbContext<EliteTaskContext>(opt =>
              {
                  opt.UseNpgsql(secretVault.GetValuesFromVault("eliteTaskConnectionString"), NpgsqlOptionsAction: sqlOption =>
                  {
                      sqlOption.MigrationsAssembly(typeof(Startup).Assembly.GetName().Name);
                      sqlOption.EnableRetryOnFailure(maxRetryCount: 3,
                          maxRetryDelay: TimeSpan.FromSeconds(10), errorCodesToAdd: null);
                  });
              },
              ServiceLifetime.Scoped)
              .AddDbContext<EliteLoggerContext>(options => options.UseNpgsql(secretVault.GetValuesFromVault("eliteLogDbConnection"))); ;

            services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");
            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllHeaders",
                             builder =>
                             {
                                 builder
                                 .WithOrigins(Configuration.GetSection("Origin").Value)
                                        .AllowAnyHeader()
                                         .AllowAnyMethod()
                                        .AllowCredentials()
                                        .WithExposedHeaders("X-Pagination");

                             });
            });


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Task MICROSERVICE API", Version = "v1" });
            });

            services.AddSingleton<IHostedService, TaskNotificationServices>();

            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(AddHeaderResultServiceFilter));
                options.Filters.Add(typeof(HttpGlobalExceptionFilter));
            }).AddControllersAsServices();

            AddServices(services);

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new CorsAuthorizationFilterFactory("AllowAllHeaders"));
            });
            if (Convert.ToBoolean(Configuration.GetSection("UseOIDC").Value))
            {
                // Add authentication 
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = AuthSchemeOptions.DefaultScheme;
                    options.DefaultChallengeScheme = AuthSchemeOptions.DefaultScheme;
                })
            // Call custom authentication extension method
            .Creator(options =>
            {
                // Configure single or multiple passwords for authentication
                options.AuthKey = "OIDCScheme";
            });

                services.AddOptions();
                services.Configure<AuthConfig>(Configuration.GetSection("AuthConfig"));
                //var monitor = services.BuildServiceProvider()
                //    .GetService<IOptions<AuthConfig>>();
                //services.AddSingleton<IAuthMethod>(new AuthMethod(monitor));
                services.AddTransient<Func<IConfiguration, IAuthTokenServices>>(sp => (IConfiguration c) => new AuthTokenServices(c));
                services.AddMvc(options =>
                {
                    // All endpoints need authorization using our custom authorization filter
                    options.Filters.Add(new AuthFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build(), Configuration));
                });
            }


            return AutoFacServiceProvider(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                loggerFactory.AddLog4Net(Configuration.GetSection("Logging:Dev_Log4NetConfigFile:Name").Value);
            }
            if (env.IsProduction())
            {
                loggerFactory.AddLog4Net(Configuration.GetSection("Logging:Prod_Log4NetConfigFile:Name").Value);
            }
            if (env.IsStaging())
            {
                loggerFactory.AddLog4Net(Configuration.GetSection("Logging:Staging_Log4NetConfigFile:Name").Value);
            }

            if (!env.IsDevelopment())
            {
                app.Use(async (context, next) =>
                {
                    context.Response.Headers.Add("Strict-Transport-Security", "max-age=15724800; includeSubDomains");
                    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self'; font-src 'self';");
                    context.Response.Headers.Add("Referrer-Policy", "no-referrer");
                    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                    context.Response.Headers.Add("X-Frame-Options", "DENY"); await next();
                });
            }
            app.UseCors("AllowAllHeaders");

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TASK MICROSERVICE API");
            });
            app.UseAuthentication();
            app.UseMvc();
        }

        private IServiceProvider AutoFacServiceProvider(IServiceCollection services)
        {
            var container = new ContainerBuilder();
            container.Populate(services);
            container.RegisterModule(new MedatorModule());
            container.RegisterModule(new ApplicationModule());
            return new AutofacServiceProvider(container.Build());
        }

        private void AddServices(IServiceCollection services)
        {
            services
                .AddSingleton<IHTMLReadUtility, HTMLReadUtility>()

                .AddTransient<IRequestContext, RequestContext>()
                .AddTransient<Func<IConfiguration, IAttachmentService>>(sp => (IConfiguration c) => new AttachmentService(c))
                .AddTransient<Func<IConfiguration, IRequestContext, IMeetingTaskService>>(sp => (IConfiguration c, IRequestContext r) => new MeetingTaskService(c, r))
                .AddTransient<Func<IConfiguration, IRequestContext, IUserService>>(sp => (IConfiguration c, IRequestContext r) => new UserService(c, r))
                .AddTransient<Func<IConfiguration, IRequestContext, ITopicServices>>(sp => (IConfiguration c, IRequestContext r) => new TopicServices(c, r))
                .AddTransient<Func<IConfiguration, IRequestContext, IJiraService>>(sp => (IConfiguration c, IRequestContext r) => new JiraService(c, r))
                .AddTransient<IHttpContextAccessor, HttpContextAccessor>()
                .AddTransient<Func<DbConnection, IEventStoreService>>(sp => (DbConnection c) => new EventStoreService(c))
                .AddTransient<Func<DbConnection, IFiltersService>>(sp => (DbConnection c) => new FiltersService(c))
                .AddTransient<Func<DbConnection, IFilterQueries>>(sp => (DbConnection c) => new FiltersService(c))
                .AddTransient<ILogException, LogExcepion>()
                .AddTransient<ITaskLog, TaskLog>();
        }
    }
}
