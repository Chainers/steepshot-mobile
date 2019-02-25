using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SteemDataScraper.Extensions;

namespace SteemDataScraper
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
            services.AddServices();

            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    options.CheckConsentNeeded = context => true;
            //    options.MinimumSameSitePolicy = SameSiteMode.None;
            //});

            services.AddMvc(o =>
                {
                    o.MaxModelValidationErrors = 50;
                    o.ValueProviderFactories.Insert(0, new SnakeCaseValueProviderFactory());
                })
                .AddRazorPagesOptions(options => { options.Conventions.AuthorizePage("/Auth"); })
                .AddJsonOptions(o =>
                {
                    o.SerializerSettings.Formatting = Formatting.Indented;
                    o.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Include;
                    o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    o.SerializerSettings.ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new SnakeCaseNamingStrategy(),
                    };
                });

            //services
            //    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            //    .AddCookie();

            //services.AddDistributedMemoryCache();

            //services.AddSession(options =>
            //{
            //    options.IdleTimeout = TimeSpan.FromSeconds(30);
            //    options.Cookie.HttpOnly = true;
            //    options.Cookie.IsEssential = true;
            //});
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.AddLogger();

                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.RunOnceServices();
            app.AddServisesToQueue();

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            //app.UseCookiePolicy();
            //app.UseSession();
            //app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Auth}/{action=Index}/{id?}");
            });
        }
    }
}
