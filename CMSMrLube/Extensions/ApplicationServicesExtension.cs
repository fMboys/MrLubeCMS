using CMS.Core.Interfaces;
using CMS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MrLubeCMS.CustomHandler;
using System.Security.Claims;

namespace MrLubeCMS.Extensions
{
    public static class ApplicationServicesExtension
    {
        public static IServiceCollection ApplicationService(this IServiceCollection services, IConfiguration configuration)
        {
            
            string ConenctionString = configuration.GetConnectionString("CmsConnection");
            //services.AddDbContext<CMSDbContext>(options =>
            //options.UseMySQL(ConenctionString));
            // Add services to the container.
            services.AddMvcCore();
            services.AddMvc(options => options.EnableEndpointRouting = false);
            services.AddAuthentication("CookieAuthentication")
                .AddCookie("CookieAuthentication", config =>
                {
                    config.Cookie.Name = "MrLubeCMS";
                    config.LoginPath = "/";
                    config.AccessDeniedPath = "/UserLoginModel/AccessDenied";
                    config.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                    config.SlidingExpiration = false;
                    config.ReturnUrlParameter.Replace("/", "dd");
                });
            services.AddAuthorization(config =>
            {
                config.AddPolicy("UserPolicy", policyBuilder =>
                {
                    policyBuilder.UserRequireCustomClaim(ClaimTypes.Email);
                    policyBuilder.UserRequireCustomClaim(ClaimTypes.Role);
                });
            });
            services.AddScoped<IAuthorizationHandler, PoliciesAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, RolesAuthorizationHandler>();
            services.AddScoped<IbannerService, bannerService>();
            return services;
        }
    }
}
