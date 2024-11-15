using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQWeb.Watermark.BackgroundService;
using RabbitMQWeb.Watermark.Models;
using RabbitMQWeb.Watermark.Services;
using System.Configuration;

namespace RabbitMQWeb.Watermark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<DataContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName: "productDb");
            });

            builder.Services.AddSingleton(sp => new ConnectionFactory()
            {
                Uri = new Uri(builder.Configuration.GetConnectionString("RabbitMQ")), DispatchConsumersAsync = true
            });

            builder.Services.AddSingleton<RabbitMQService>();
            builder.Services.AddSingleton<RabbitMQPublisher>();

            builder.Services.AddHostedService<ImageWatermarkProcessBackgroundService>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
