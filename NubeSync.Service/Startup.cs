using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Nube.SampleService.Hubs;
using NubeSync.Core;
using NubeSync.Server;
using NubeSync.Service.Data;
using NubeSync.Service.DTO;

namespace NubeSync.Service
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // UNCOMMENT THIS IF YOU WANT TO ACTIVATE AUTHENTICATION
            //services.AddMicrosoftIdentityWebApiAuthentication(Configuration);

            var dbConnectionString = Configuration["ConnectionStrings:DefaultConnection"].ToString();

            //services.AddDbContext<DataContext>(options => options.UseSqlServer(dbConnectionString));
            services.AddDbContextPool<DataContext>(options => options.UseMySql(dbConnectionString, ServerVersion.AutoDetect(dbConnectionString),
                b => b.MigrationsAssembly("NubeSync.Service")));

            // services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("test"));

            services.AddControllers();
            services.AddSignalR();

            services.AddCors(o => o.AddPolicy("AllowedOrigins", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            services.AddTransient<IAuthentication, Authentication>();
            services.AddTransient<IChangeTracker, ChangeTracker>();
            services.AddTransient<IOperationService>(s => new OperationService(typeof(TodoItem)));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "NubeSync.Service", Version = "v1" });
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "NubeSync.Service v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("AllowedOrigins");

            // UNCOMMENT THIS IF YOU WANT TO ACTIVATE AUTHENTICATION
            //app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<UpdateHub>("/updateHub");
            });
        }
    }
}
