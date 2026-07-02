using Application.Repositories;
using Application.Services.Interfaces;
using Application.Services.SecurityServices;
using Application.Services.UserService;
using Infrastructure.DataAccess;
using Infrastructure.Repositories.UsersRepository;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Text.Json.Serialization;

namespace Presentation.Services.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();

            return services;
        }

        public static IServiceCollection AddPresentation(this IServiceCollection services)
        {
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "YaEvents", Version = "v1" });

                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    Description = "Введите токен JWT в формате: Bearer {token}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    BearerFormat = "JWT"
                });

                options.AddSecurityRequirement(document => new()
                {
                    [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document)] = []
                });

            });

            return services;
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, WebApplicationBuilder builder)
        {

            services.AddScoped<IUsersRepository, UsersRepository>();
            services.AddScoped<ISecurityServise, SecurityServise>();
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            return services;
        }
    }
}
