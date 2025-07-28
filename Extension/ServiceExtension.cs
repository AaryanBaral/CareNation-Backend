using System.Text;
using backend.Configurations;
using backend.Data;
using backend.ExceptionHandling;
using backend.Interface;
using backend.Interface.Repository;
using backend.Interface.Service;
using backend.Models;
using backend.Repository;
using backend.Service;
using backend.Service.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace backend.Extension
{
    public static class ServiceExtension
    {
        public static void AddAppServices(this IServiceCollection service, IConfiguration configuration)
        {
            service.AddDatabase(configuration);
            service.AddServices();
            service.AddRepository();
            service.AddIdentity();
            service.AddMiddlewareFiles();
            service.AddCorsConfiguration();
            service.AddJwtAuthentication(configuration);
            service.AddExceptionHandler<GlobalExceptionHandling>();
        }
        public static void AddDatabase(this IServiceCollection service, IConfiguration configuration)
        {
            Console.WriteLine(configuration.GetConnectionString("DefaultConnection"));
            service.AddDbContext<AppDbContext>(options =>
            options.UseMySQL(configuration.GetConnectionString("DefaultConnection")!));
        }
        private static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
        {
            services.AddCors(options =>
            {

                options.AddPolicy("AllowAny", builder =>
                {
                    builder.WithOrigins("http://localhost:5173", "https://localhost:5173", "http://localhost:5174")
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials();
                });
            });
            return services;
        }
        public static void AddRepository(this IServiceCollection service)
        {
            service.AddScoped<ICategoryRepository, CategoryRepository>();
            service.AddScoped<IUserRepository, UserRepository>();
            service.AddScoped<IProductRepository, ProductRepository>();
            service.AddScoped<IOrderItemRepository, OrderItemRepository>();
            service.AddScoped<IOrderRepository, OrderRepository>();
            service.AddScoped<IProductImageRepository, ProductImageRepository>();
            service.AddScoped<ICartRepository, CartRepository>();
            service.AddScoped<IDistributorRepository, DistributorRepository>();
            service.AddScoped<IReportRepository, ReportRepository>();
            service.AddScoped<ICommissionPayoutRepository, CommissionPayoutRepository>();
        }
        public static void AddServices(this IServiceCollection service)
        {
            service.AddScoped<ICategoryService, CategoryService>();
            service.AddScoped<IUserService, UserService>();
            service.AddScoped<IProductService, ProductService>();
            service.AddScoped<IOrderService, OrderService>();
            service.AddScoped<IProductImageService, ProductImageService>();
            service.AddScoped<IOrderItemService, OrderItemService>();
            service.AddScoped<IJwtService, JwtService>();
            service.AddScoped<ICartService, CartService>();
            service.AddScoped<IDistributorService, DistributorService>();
            service.AddScoped<IReportService, ReportService>();
            service.AddScoped<ICommissionPayoutService, CommissionPayoutService>();

        }

        public static void AddIdentity(this IServiceCollection service)
        {
            service.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
        }
        public static void AddMiddlewareFiles(this IServiceCollection service)
        {
            service.AddScoped<IUnitOfWork, UnitOfWork>();
        }
            public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind and validate your JWT config
        services.Configure<JwtConfig>(configuration.GetSection("JwtConfig"));

        var secret = configuration["JwtConfig:Secret"]
                     ?? throw new NullReferenceException("Jwt secret is null");

        var key = Encoding.ASCII.GetBytes(secret);

        // Add JWT Bearer authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero, // optional: disable expiration delay
                    ValidateLifetime = true
                };
            });
    }

    }
}