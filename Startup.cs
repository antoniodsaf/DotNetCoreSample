using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Zema.Service;
using AspectCore.Extensions.DependencyInjection;
using SmartSql.ConfigBuilder;
using SmartSql.DIExtension;
using SmartSql.InvokeSync;
using Microsoft.EntityFrameworkCore.Metadata;
using Swashbuckle.AspNetCore.Swagger;

namespace Zema
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Add Hangfire services.
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(Configuration.GetConnectionString("HangfireConnection"), new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    UsePageLocksOnDequeue = true,
                    DisableGlobalLocks = true
                }));

            // Add the processing server as IHostedService
            services.AddHangfireServer();

            services.AddHealthChecksUI()
            .AddHealthChecks()
            .AddCheck<RandomHealthCheck>("random")
            .AddSqlServer(
              connectionString: "Server=localhost;Database=tempdb;User Id=sa;Password=c0mPlexP4ssword!;",
              healthQuery: "SELECT Id, StateId, StateName, InvocationData, Arguments, CreatedAt, ExpireAt FROM tempdb.HangFire.Job;",
              name: "sql",
              failureStatus: HealthStatus.Degraded,
              tags: new string[] { "db", "sql", "sqlserver" });

            services
                .AddSmartSql((sp, builder) =>
                {
                    builder.UseProperties(Configuration.AsEnumerable());
                })
                .AddRepositoryFromAssembly(o =>
                {
                    o.AssemblyString = "Zema";
                    o.Filter = (type) => type.Namespace == "Zema.DyRepositories";
                });

            services.AddSingleton<UserService>();
            RegisterConfigureSwagger(services);
            return services.BuildAspectInjectorProvider();

        }

        private void RegisterConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "Zema.Sample.AspNetCore",
                    Version = "v1",
                    Contact = new Contact
                    {
                        Url = "https://github.com/Neppo",
                        Email = "antonio.filho@neppo.com.br",
                        Name = "Neppo"
                    },
                    Description = "Zema.Sample.AspNetCore"
                });
                c.CustomSchemaIds((type) => type.FullName);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IBackgroundJobClient backgroundJobs, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            /* app.ApplicationServices.UseSmartSqlSync();
            app.ApplicationServices.UseSmartSqlSubscriber((syncRequest) =>
            {
                Console.Error.WriteLine(syncRequest.Scope);
            });*/

            app.UseMvc();
            
            

            app.UseHangfireDashboard();
            backgroundJobs.Enqueue(() => Console.WriteLine("Hello world from Hangfire!"));


            var manager = new RecurringJobManager();
            manager.AddOrUpdate("hello-world-id", () => Console.WriteLine("Hello World!"), Cron.Minutely());

            app.UseHealthChecks("/healthz", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            })
            .UseHealthChecksUI();

            app.UseSwagger(c => { });
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Zema.Sample.AspNetCore"); });
            
        }
    }


}
