using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Dapper;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using MissingPersonApp.Data;
using System.Data;
using Microsoft.AspNetCore.Diagnostics;

namespace MissingPersonApp
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddDbContext<MyDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("MyDatabase")));

            services.AddScoped<IDbConnection>(provider =>
                new NpgsqlConnection(Configuration.GetConnectionString("MyDatabase")));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Missing Person API", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // app.UseDeveloperExceptionPage();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                name: "PostBio",
                pattern: "api/bio/{id}",
                defaults: new { controller = "Bio", action = "PostBioAsync" });
                
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                name: "PostRelative",
                pattern: "api/relative/{id}",
                defaults: new { controller = "Relative", action = "PostRelativeAsync" });

                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                name: "PostKron",
                pattern: "api/kron/{id}",
                defaults: new { controller = "Kron", action = "PostKronAsync" });

                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                name: "PutBio",
                pattern: "api/bio/{id}",
                defaults: new { controller = "Bio", action = "PutBioAsync" });

                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                name: "PutRelative",
                pattern: "api/kontak/{id}",
                defaults: new { controller = "Relative", action = "PutRelativeAsync" });

                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                name: "PutKron",
                pattern: "api/kron/{id}",
                defaults: new { controller = "Kron", action = "PutKronAsync" });
                
                
            }
            
            

            );

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Missong Person API v1");
            });
        }
    }
}