using Application.Cache;
using Application.Repositories;
using Application.Services.EventService;
using Application.Services.Interfaces;
using Infrastructure.BackgroundServices;
using Infrastructure.CasheRepositories;
using Infrastructure.Consumer;
using Infrastructure.Consumer.Interfaces;
using Infrastructure.DataAccess;
using Infrastructure.Repositories.EventsRepository;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Repositories.MessageRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StackExchange.Redis;
using System.Text.Json.Serialization;

namespace Presentation.Services.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IEventService, EventService>();

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
            services.AddScoped<IEventsRepository, EventsRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            services.AddSingleton<IEventsConsumer, EventsConsumer>();
            services.AddSingleton<TopicBootstrapper>();

            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(GetRedisConfigurationOptions(builder.Configuration)));
            services.AddScoped<ICasheRepository, RedisCasheRepository>();

            services.AddHostedService<EventsBackgroundService>();

            return services;
        }

        public static ConfigurationOptions GetRedisConfigurationOptions(IConfiguration configuration)
        {
            var redisSection = configuration.GetSection("Redis");
            if (!int.TryParse(redisSection["ConnectTimeout"], out int connectionTime))
            {
                connectionTime = 5000;
            }
            if (!int.TryParse(redisSection["SyncTimeout"], out int syncTime))
            {
                syncTime = 3000;
            }
            if(!int.TryParse(redisSection["ConnectRetry"], out int connectRetry))
            {
                connectRetry = 3;
            }
            var password = redisSection["Password"];
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("В конфигурации не передано значение пароля для Redis");

            return new ConfigurationOptions
            {
                EndPoints = { redisSection["Connection"] ?? "localhost:6379" },
                Password = password,
                ConnectTimeout = connectionTime,
                SyncTimeout = syncTime,
                AbortOnConnectFail = false,
                ConnectRetry = connectRetry
            };
        }
        public static IServiceCollection AddObservability(this IServiceCollection services, WebApplicationBuilder builder)
        {
            services.AddOpenTelemetry()
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddOtlpExporter(o => o.Endpoint = new Uri(builder.Configuration["Otlp:Endpoint"]!)))
                .WithMetrics(metrics => metrics
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddPrometheusExporter())
                .ConfigureResource(r => r.AddService(serviceName: "events"));


            return services;
        }
    }
}
