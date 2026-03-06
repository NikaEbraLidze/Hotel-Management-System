using Microsoft.AspNetCore.Builder;
using hms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using hms.Domain.Identity;
using System;
using System.Threading.Tasks;
using hms.Infrastructure.Persistence.Seeding;
using hms.Infrastructure.Repository;
using hms.Application.Contracts.Service;
using hms.Application.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Mapster;
using MapsterMapper;
using hms.Application.Mapping;
using hms.Api.Swagger;
using hms.Api.Middlewares;

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

            #region Mapster
            var mapsterConfig = new TypeAdapterConfig();
            mapsterConfig.Scan(typeof(MappingAssemblyMarker).Assembly);

            builder.Services.AddSingleton(mapsterConfig);
            builder.Services.AddScoped<MapsterMapper.IMapper, ServiceMapper>();
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

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter JWT token. You can paste only the token or use: Bearer {token}"
                });

                options.OperationFilter<AuthorizeOperationFilter>();
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

            #region Repositories
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            #endregion

            #region Services
            builder.Services.AddScoped<IJWTTokenGenerator, JWTTokenGenerator>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            #endregion

            #region JWT Authentication
            var issuer = builder.Configuration["JwtOptions:Issuer"];
            var secret = builder.Configuration["JwtOptions:Secret"];
            var audience = builder.Configuration["JwtOptions:Audience"];
            var key = Encoding.ASCII.GetBytes(secret);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidIssuer = issuer,
                    ValidAudience = audience
                };
            });

            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(3);
            });
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

            app.UseMiddleware<GlobalExceptionMiddleware>();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            #endregion

            await app.RunAsync();
        }
    }
}
