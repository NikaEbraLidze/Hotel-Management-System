using Microsoft.AspNetCore.Builder;
using hms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using hms.Infrastructure.Persistence.Identity;
using System;
using System.Threading.Tasks;
using hms.Infrastructure.Persistence.Seeding;

namespace hms.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Controllers
            builder.Services.AddControllers();
            #endregion

            #region Swagger
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "HMS API",
                    Version = "v1",
                    Description = "Hotels Management System (HMS) API"
                });
            });
            #endregion

            #region DbContext
            builder.Services.AddDbContext<HmsDbContext>(options =>
            {
                var cs = builder.Configuration.GetConnectionString("HmsDb");
                options.UseSqlServer(cs);
            });
            #endregion

            #region Identity

            builder.Services
                .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
                {
                    options.Password.RequiredLength = 6;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                })
                .AddEntityFrameworkStores<HmsDbContext>()
                .AddDefaultTokenProviders();

            #endregion

            var app = builder.Build();

            #region Middleware Pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            if (app.Environment.IsDevelopment())
            {
                using var scope = app.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<HmsDbContext>();
                await db.Database.MigrateAsync();
                await HmsDbSeeder.SeedAsync(app.Services);
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            #endregion

            await app.RunAsync();
        }
    }
}
