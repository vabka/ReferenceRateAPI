using System;
using ExchangeAPI.Data;
using ExchangeAPI.HostedServices;
using ExchangeAPI.Service;
using ExchangeRateLoader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExchangeAPI
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();

            services.AddDbContext<ReferenceRatesDbContext>(
                (s, o) =>
                {
                    var conf = s.GetRequiredService<IConfiguration>().GetSection("Database");
                    var type = conf.GetValue<DatabaseType>("Type");
                    var connectionString = conf.GetValue<string>("ConnectionString");

                    switch (type)
                    {
                        case DatabaseType.SqlServer:
                            o.UseSqlServer(connectionString);
                            break;
                        case DatabaseType.Sqlite:
                            o.UseSqlite(connectionString);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }, ServiceLifetime.Transient,
                ServiceLifetime.Singleton
            );

            services.AddHttpClient();
            services.AddSingleton(s =>
            {
                var conf = s.GetRequiredService<IConfiguration>().GetSection("ReferenceRates");
                var history = conf.GetValue<string>("HistoricalDataUri");
                var fresh = conf.GetValue<string>("FreshDataUri");
                return new LoaderConfig
                {
                    HistoricalDataUri = history,
                    FreshDataUri = fresh
                };
            });
            services.AddSingleton<Loader>();
            services.AddHostedService<LoaderHostedService>();

            services.AddScoped<ReferenceRateService>();

            services.AddControllers();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
