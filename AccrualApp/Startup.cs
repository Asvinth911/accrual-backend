using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccrualApp.DBModels;
using AccrualApp.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AccrualApp
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
            //services.AddDbContext<aci_databaseContext>(op => op.UseSqlServer("Server=tcp:aci-database-server.database.windows.net,1433;Database=aci_database;User ID=cslabs-admin;Password=Labs@CS#1192;Encrypt=True;TrustServerCertificate=False;"));

             services.AddDbContext<aci_databaseContext>(options => options.UseSqlServer("Server=tcp:aci-database-server.database.windows.net,1433;Initial Catalog=aci_database;Persist Security Info=False;User ID=cslabs-admin;Password=Labs@CS#1192;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"));


            //services.AddTransient<IRepository, TransactionRepository>();



            services.AddControllers();

            

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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
