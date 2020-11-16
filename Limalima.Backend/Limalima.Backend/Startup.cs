using Limalima.Backend.Azure;
using Limalima.Backend.Components;
using Limalima.Backend.Components.ParsingClient;
using Limalima.Backend.Data;
using Limalima.Backend.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Limalima.Backend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var mvc = services.AddControllersWithViews();
            mvc.AddRazorRuntimeCompilation();

            var sqlConnectionString = Configuration.GetConnectionString("Limalima-Database");

            services.AddDbContext<ArtDbContext>(options => options.UseNpgsql(sqlConnectionString),
                ServiceLifetime.Transient);

            services.AddScoped<IImageValidator, ImageValidator>();
            services.AddScoped<IDataImportLinkValidator, DataImportLinkValidator>();
            services.AddScoped<IAzureImageUploadComponent, AzureImageUploadComponent>();
            services.AddScoped<IWatermarkService, WatermarkService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
