using InstaminiWebService.Database;
using InstaminiWebService.ResponseModels.Factory;
using InstaminiWebService.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;

namespace InstaminiWebService
{
    public class Startup
    {
        const string TOKEN_BASED_AUTH_SCHEME = "TokenBased";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(TOKEN_BASED_AUTH_SCHEME)
                    .AddScheme<TokenAuthenticationOptions, TokenAuthenticationHandler>(TOKEN_BASED_AUTH_SCHEME, 
                        options =>
                        {
                        });
            services.AddControllers();
            services.AddDbContext<InstaminiContext>(options => {
                            options.UseMySQL(Configuration.GetConnectionString("Instamini"));
                            options.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()));
                        })
                    .AddSingleton<IResponseModelFactory>(new ResponseModelFactory());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                                    Configuration.GetValue("AvatarServingAbsolutePath", 
                                        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"))),
                RequestPath = "/avatars"
            }).UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                                    Configuration.GetValue("PhotoServingAbsolutePath",
                                        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"))),
                RequestPath = "/photos"
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
