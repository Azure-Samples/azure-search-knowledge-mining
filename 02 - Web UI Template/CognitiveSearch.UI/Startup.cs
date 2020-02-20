using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CognitiveSearch.UI.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace CognitiveSearch.UI
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                //options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            //var appInsightsConfig = new AppInsightsConfig
            //{
            //    InstrumentationKey = Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]
            //};
            //services.AddSingleton(appInsightsConfig);
            //services.AddApplicationInsightsTelemetry(appInsightsConfig.InstrumentationKey);

            var apiConfig = new ApiConfig
            {
                BaseUrl = "" //Configuration["ApiUrl"]
            };
            services.AddSingleton(apiConfig);

            var orgConfig = new OrganizationConfig
            {
                Logo = Configuration["OrganizationLogo"],
                Name = Configuration["OrganizationName"],
                Url = Configuration["OrganizationWebSiteUrl"],
            };
            services.AddSingleton(orgConfig);

            var appConfig = new AppConfig
            {
                ApiConfig = apiConfig,
                //AppInsights = appInsightsConfig,
                Organization = orgConfig,
                Customizable = true // Configuration["Customizable"].Equals("true", StringComparison.InvariantCultureIgnoreCase)
            };
            services.AddSingleton(appConfig);

            services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")));

            services.AddMvc(); //.SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
