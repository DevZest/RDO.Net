using DevZest.Data.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Movies.AspNetCore
{
    public class Startup
    {
        private const string CONTENT_ROOT_PATH = "%CONTENTROOTPATH%";

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Env { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc(option => option.EnableEndpointRouting = false)
                .AddDataSetMvc();    // Add DataSet MVC support

            // Inject Db and IDataSetHtmlGenerator
            var connectionString = Configuration.GetConnectionString("Movies");
            if (Env.IsDevelopment())
            {
                if (connectionString.Contains(CONTENT_ROOT_PATH))
                    connectionString = connectionString.Replace(CONTENT_ROOT_PATH, Env.ContentRootPath);
            }
            services.AddScoped(serviceProvider => new Db(connectionString));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc();
        }
    }
}
