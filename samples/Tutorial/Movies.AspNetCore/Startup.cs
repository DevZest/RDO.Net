using DevZest.Data.AspNetCore;
using DevZest.Data.AspNetCore.Primitives;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Movies.AspNetCore
{
    public class Startup
    {
        private const string CONTENT_ROOT_PATH = "%CONTENTROOTPATH%";

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public IConfiguration Configuration { get; }

        public IHostingEnvironment Env { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc()
                .AddDataSetMvc()    // Add DataSet MVC support
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Inject Db and IDataSetHtmlGenerator
            var connectionString = Configuration.GetConnectionString("Movies");
            if (Env.IsDevelopment())
            {
                if (connectionString.Contains(CONTENT_ROOT_PATH))
                    connectionString = connectionString.Replace(CONTENT_ROOT_PATH, Env.ContentRootPath);
            }
            services.AddScoped(serviceProvider => new Db(connectionString));
            services.AddSingleton<IDataSetHtmlGenerator, DefaultDataSetHtmlGenerator>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
