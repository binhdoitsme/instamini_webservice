using InstaminiWebService.Database;
using InstaminiWebService.ModelWrappers.Factory;
using InstaminiWebService.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
                            options.EnableSensitiveDataLogging();
                        })
                    .AddSingleton<IModelWrapperFactory>(new ModelWrapperFactory());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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
