using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;

namespace test__ts
{
    public class Startup
    {
        public Startup(Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile($"appsettings.{System.Environment.GetEnvironmentVariable("myCustomParam")}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
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

            Test().GetAwaiter().GetResult();
        }

        private async Task Test()
        {
            var endpoint = Configuration.GetValue<string>("Connection:Endpoint");
            var key = Configuration.GetValue<string>("Connection:Key");
            var client = new CosmosClient(endpoint, key);

            Database database = await client.CreateDatabaseIfNotExistsAsync("test");
            Container container = await database.CreateContainerIfNotExistsAsync("test", "/pk");

            var item = new Item()
            {
                pk = "pk-1",
                value = DateTime.UtcNow,
                id = Guid.NewGuid()
            };

            Console.WriteLine(item);
            var res = await container.CreateItemAsync<Item>(item, new PartitionKey(item.pk));
            Console.WriteLine(item);
            Console.WriteLine(res);
        }
    }

    public class Item
    {
        public string pk {get;set;}

        public DateTime value {get;set;}

        public Guid id {get;set;}
    }
}
